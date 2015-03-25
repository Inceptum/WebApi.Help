using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// An API explorer implementation which takes into account current culture on API documentation generation
    /// </summary>
    public sealed class CultureAwareApiExplorer : IApiExplorer
    {
        /*
         * Default ApiExplorer implementation caches api descriptions once they're generated and doesn't allow to reset this cache.
         * As a workaround we'll have a separate ApiExplorer for each known culture.
         */
        private readonly ConcurrentDictionary<CultureInfo, IApiExplorer> m_Cache;
        private readonly HttpConfiguration m_Configuration;

        public CultureAwareApiExplorer(HttpConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            m_Configuration = configuration;

            m_Cache = new ConcurrentDictionary<CultureInfo, IApiExplorer>();
            m_Cache[CultureInfo.InvariantCulture] = new ApiExplorer(configuration);
            m_Cache[new CultureInfo("en")] = new ApiExplorer(configuration);
            m_Cache[new CultureInfo("ru-RU")] = new ApiExplorer(configuration);
        }

        public Collection<ApiDescription> ApiDescriptions
        {
            get
            {
                return m_Cache.GetOrAdd(Thread.CurrentThread.CurrentUICulture, _ => new ApiExplorer(m_Configuration)).ApiDescriptions;
            }
        }
    }
}