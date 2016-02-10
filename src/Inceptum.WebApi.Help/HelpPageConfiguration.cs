using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using Inceptum.WebApi.Help.Builders;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// This class provides fluent interface for help page configuration
    /// </summary>
    public sealed class HelpPageConfiguration
    {
        private readonly HttpConfiguration m_HttpConfiguration;
        private string m_RouteBaseUri = "/help";
        private Uri m_SamplesBaseUri = new Uri(string.Format("http://{0}", Environment.MachineName ?? "localhost"));
        private Type[] m_AutoDocumentedTypes = new Type[0];
        private string m_AutoDocumentedTypesTocPath;
        private Action<HelpProvider> m_HelpProviderSetup;
        private readonly ServiceLocatorImpl m_ServiceLocator;
        private readonly List<Tuple<IPdfTemplateProvider, int>> m_PdfTemplateProviders = new List<Tuple<IPdfTemplateProvider, int>>();

        public HelpPageConfiguration(HttpConfiguration httpConfiguration)
        {
            if (httpConfiguration == null) throw new ArgumentNullException("httpConfiguration");
            m_HttpConfiguration = httpConfiguration;
            var srvLocator = new ServiceLocatorImpl();
            srvLocator.Register<IExtendedApiExplorer>(() => new ExtendedApiExplorer(m_HttpConfiguration) { IgnoreUndeclaredRouteParameters = true });
            srvLocator.Register<IExtendedDocumentationProvider>(() => new XmlDocumentationProvider());
            srvLocator.Register<IContentProvider>(() => new LocalizableContentProvider(new EmbeddedResourcesContentProvider()));
            srvLocator.Register(createDefaultHelpProvider);
            m_ServiceLocator = srvLocator;
        }

        /// <summary>
        /// Gets and instance of the service locator        
        /// </summary>
        public IServiceLocator ServiceLocator
        {
            get { return m_ServiceLocator; }
        }

        internal Uri SamplesUriInternal
        {
            get { return m_SamplesBaseUri; }
        }

        /// <summary>
        /// Configures a base uri for help page requests.        
        /// Valid example are /api/help/{*resource}, /apihelp/{*resource}, /help/{*resource}.
        /// </summary>
        public HelpPageConfiguration Route(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException(uri);
            if (!uri.StartsWith("/")) throw new ArgumentException(string.Format("The '{0}' is not valid value for route base URI", uri));

            m_RouteBaseUri = new Uri(uri, UriKind.Relative).ToString().TrimEnd('/', '\\');

            return this;
        }

        /// <summary>
        /// Configures an uri to be used as base address for sample routes generation 
        /// </summary> 
        public HelpPageConfiguration SamplesUri(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException(@"Value expected to be an absolute uri, e.g. http://api.example.com", "uri");
            m_SamplesBaseUri = uri;
            return this;
        }

        public HelpPageConfiguration WithApiExplorer(IExtendedApiExplorer apiExplorer)
        {
            if (apiExplorer == null) throw new ArgumentNullException("apiExplorer");
            m_ServiceLocator.Register(() => apiExplorer);
            return this;
        }

        public HelpPageConfiguration WithDocumentationProvider(IExtendedDocumentationProvider documentationProvider)
        {
            if (documentationProvider == null) throw new ArgumentNullException("documentationProvider");
            m_ServiceLocator.Register(() => documentationProvider);
            return this;
        }

        public HelpPageConfiguration WithXmlDocumentationProvider(string docsFolder)
        {
            if (string.IsNullOrWhiteSpace(docsFolder)) throw new ArgumentNullException("documentationFolder");
            m_ServiceLocator.Register(() => new XmlDocumentationProvider(docsFolder));
            return this;
        }

        public HelpPageConfiguration WithContentProvider(IContentProvider contentProvider)
        {
            if (contentProvider == null) throw new ArgumentNullException("contentProvider");
            m_ServiceLocator.Register(() => contentProvider);
            return this;
        }

        public HelpPageConfiguration WithHelpProvider(IHelpProvider helpProvider)
        {
            if (helpProvider == null) throw new ArgumentNullException("helpProvider");
            m_ServiceLocator.Register(() => helpProvider);
            return this;
        }

        public HelpPageConfiguration ConfigureHelpProvider(Action<HelpProvider> setup)
        {
            if (setup == null) throw new ArgumentNullException("setup");
            m_HelpProviderSetup = setup;
            return this;
        }

        public HelpPageConfiguration AutoDocumentedTypes(IEnumerable<Type> types, string tocPath = null)
        {
            if (types == null) throw new ArgumentNullException("types");
            m_AutoDocumentedTypes = types.ToArray();
            m_AutoDocumentedTypesTocPath = tocPath;
            return this;
        }

        public HelpPageConfiguration RegisterPdfTemplateProvider(IPdfTemplateProvider provider, int rank = 0)
        {
            if (provider == null) throw new ArgumentNullException("provider");

            if (m_PdfTemplateProviders.Any(x => x.GetType() == provider.GetType()))
            {
                throw new InvalidOperationException(string.Format("Provider of type {0} is already registered.", provider.GetType()));
            }

            m_PdfTemplateProviders.Add(Tuple.Create(provider, rank));
            return this;
        }

        public HelpPageConfiguration UnregisterPdfTemplateProvider(Type concreteType)
        {
            if (concreteType == null) throw new ArgumentNullException("concreteType");

            var builder = findPdfTemplateProvider(concreteType);
            if (builder != null)
            {
                m_PdfTemplateProviders.Remove(builder);
            }
            return this;
        }

        private Tuple<IPdfTemplateProvider, int> findPdfTemplateProvider(Type actualType)
        {
            if (actualType == null) throw new ArgumentNullException("actualType");

            return m_PdfTemplateProviders.FirstOrDefault(x => x.Item1.GetType() == actualType);
        }

        internal void WireUp()
        {
            m_HttpConfiguration.Services.Replace(typeof(IApiExplorer), m_ServiceLocator.Get<IExtendedApiExplorer>());
            m_HttpConfiguration.Services.Replace(typeof(IDocumentationProvider), m_ServiceLocator.Get<IExtendedDocumentationProvider>());
            m_HttpConfiguration.MessageHandlers.Insert(0, new HelpPageHandler(m_RouteBaseUri, m_ServiceLocator.Get<IHelpProvider>(), m_ServiceLocator.Get<IContentProvider>(), m_PdfTemplateProviders));
        }

        private IHelpProvider createDefaultHelpProvider()
        {
            var helpProvider = new HelpProvider();
            helpProvider.RegisterBuilder(new ApiDocumentationBuilder(this));
            helpProvider.RegisterBuilder(new ErrorsDocumentationBuilder());
            if (m_HelpProviderSetup != null)
            {
                m_HelpProviderSetup(helpProvider);
            }
            if (m_AutoDocumentedTypes.Length != 0)
            {
                helpProvider.RegisterBuilder(new TypesDocumentationBuilder(m_HttpConfiguration, m_AutoDocumentedTypes, m_AutoDocumentedTypesTocPath ?? TypesDocumentationBuilder.DEFAULT_TOC_PATH));
            }
            return helpProvider;
        }
    }

    /// <summary>
    /// This class is used by help page infrastructure to resolve service dependencies 
    /// </summary>
    public interface IServiceLocator
    {
        T Get<T>() where T : class;
    }

    internal sealed class ServiceLocatorImpl : IServiceLocator
    {
        readonly Dictionary<Type, Func<object>> m_ObjectFactories = new Dictionary<Type, Func<object>>();

        public void Register<T>(Func<T> creator) where T : class
        {
            if (creator == null) throw new ArgumentNullException("creator");
            m_ObjectFactories[typeof(T)] = creator;
        }

        public T Get<T>() where T : class
        {
            Func<object> factory;
            if (m_ObjectFactories.TryGetValue(typeof(T), out factory))
            {
                return (T)factory();
            }
            throw new InvalidOperationException(string.Format("Uknown service '{0}'", typeof(T)));
        }
    }
}