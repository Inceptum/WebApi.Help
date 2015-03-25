using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Inceptum.WebApi.Help.Builders;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Help page configuration
    /// </summary>
    public sealed class HelpPageConfiguration
    {
        private readonly HttpConfiguration m_HttpConfiguration;
        private IContentProvider m_CustomContentProvider;
        private IHelpProvider m_CustomHelpProvider;
        private readonly List<Tuple<IPdfTemplateProvider, int>> m_PdfTemplateProviders = new List<Tuple<IPdfTemplateProvider, int>>();



        public HelpPageConfiguration(HttpConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            m_HttpConfiguration = configuration;
            UriPrefix = "/help";
            createDefaultServices();
        //    RegisterPdfTemplateProvider(new DefaultPdfTemplateProvider());

        }



        private void createDefaultServices()
        {
            DefaultContentProvider = new LocalizableContentProvider(new EmbeddedResourcesContentProvider());
            var helpProvider = new HelpProvider();
            helpProvider.RegisterBuilder(new ApiDocumentationBuilder(m_HttpConfiguration));
            helpProvider.RegisterBuilder(new ErrorsDocumentationBuilder());
            m_DefaultHelpProvider = helpProvider;
            m_DefaultApiExplorer = new ExtendedApiExplorer(m_HttpConfiguration);
        }

        /// <summary>
        /// Gets or sets an uri prefix to be used by help pages, e.g. /help or /api/help
        /// </summary>        
        public string UriPrefix
        {
            get { return m_UriPrefix; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(value);
                if (!value.StartsWith("/")) throw new ArgumentException(string.Format("The '{0}' is not valid value for URI", value));

                var uri = new Uri(value, UriKind.Relative);

                m_UriPrefix = uri.ToString().TrimEnd('/', '\\');
            }
        }
        private string m_UriPrefix;

        public IContentProvider DefaultContentProvider { get; private set; }
        
        private HelpProvider m_DefaultHelpProvider;
        public IHelpProvider DefaultHelpProvider
        {
            get { return m_DefaultHelpProvider; }
        }

        private ExtendedApiExplorer m_DefaultApiExplorer;
        public IExtendedApiExplorer DefaultApiExplorer
        {
            get { return m_DefaultApiExplorer; }
        }

        #region Fluent configuration

        public HelpPageConfiguration WithApiExplorer(IExtendedApiExplorer apiExplorer)
        {
            if (apiExplorer == null) throw new ArgumentNullException("apiExplorer");
            m_HttpConfiguration.Services.Replace(typeof(IApiExplorer), apiExplorer);
            return this;
        }

        public HelpPageConfiguration WithDocumentationProvider(IDocumentationProvider documentationProvider)
        {
            if (documentationProvider == null) throw new ArgumentNullException("documentationProvider");
            m_HttpConfiguration.Services.Replace(typeof(IDocumentationProvider), documentationProvider);
            return this;
        }

        public HelpPageConfiguration WithContentProvider(IContentProvider contentProvider)
        {
            if (contentProvider == null) throw new ArgumentNullException("contentProvider");
            m_CustomContentProvider = contentProvider;
            return this;
        }

        public HelpPageConfiguration WithHelpProvider(IHelpProvider helpProvider)
        {
            if (helpProvider == null) throw new ArgumentNullException("helpProvider");
            m_CustomHelpProvider = helpProvider;
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


        public HelpPageConfiguration RegisterHelpBuilder(IHelpBuilder builder, int rank = 0)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            m_DefaultHelpProvider.UnregisterBuilder(builder.GetType());
            m_DefaultHelpProvider.RegisterBuilder(builder, rank);
            return this;
        }



        public HelpPageConfiguration UnregisterHelpBuilder<T>() where T : IHelpBuilder
        {
            m_DefaultHelpProvider.UnregisterBuilder(typeof(T));
            return this;
        }

        Type[] m_AutoDocumentedTypes;
        public HelpPageConfiguration AutoDocumentedTypes(params Type[] types)
        {
            if (types == null) throw new ArgumentNullException("types");
            m_AutoDocumentedTypes = types;
            return this;
        }

        #endregion

        internal void Configure()
        {
            // Replace default ApiExplorer (if not already replaced)
            if (m_HttpConfiguration.Services.GetApiExplorer().GetType() == typeof(ApiExplorer))
            {
                m_HttpConfiguration.Services.Replace(typeof(IApiExplorer), new ExtendedApiExplorer(m_HttpConfiguration));
            }

            if (m_AutoDocumentedTypes != null && m_AutoDocumentedTypes.Length > 0)
            {
                m_DefaultHelpProvider.RegisterBuilder(new TypesDocumentationBuilder(m_HttpConfiguration, m_AutoDocumentedTypes));
            }

            m_HttpConfiguration.MessageHandlers.Insert(0,
                new HelpPageHandler(this,
                    m_CustomHelpProvider ?? DefaultHelpProvider,
                    m_CustomContentProvider ?? DefaultContentProvider,
                    m_PdfTemplateProviders));
        }
    }
}