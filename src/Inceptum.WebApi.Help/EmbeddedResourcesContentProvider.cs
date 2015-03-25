using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inceptum.WebApi.Help.Extensions;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// A class which serves static content from Assebly's resources
    /// </summary>
    public class EmbeddedResourcesContentProvider : IContentProvider
    {
        private readonly IDictionary<string, Lazy<byte[]>> m_ResourceCache;        

        internal EmbeddedResourcesContentProvider() : this(typeof (EmbeddedResourcesContentProvider).Assembly, "Inceptum.WebApi.Help.Content.")
        {            
        }

        public EmbeddedResourcesContentProvider(Assembly resourcesAssembly, string resourcesNamespaceRoot)
        {
            if (resourcesAssembly == null) throw new ArgumentNullException("resourcesAssembly");
            if (resourcesNamespaceRoot == null) throw new ArgumentNullException("resourcesNamespaceRoot");

            m_ResourceCache = initializeResourceCache(resourcesAssembly, resourcesNamespaceRoot);
        }

        public virtual StaticContent GetContent(string resourcePath)
        {
            if (resourcePath == null) throw new ArgumentNullException("resourcePath");

            resourcePath = resourcePath.Replace('/', '.').Replace('\\', '.'); // To resource name compatible format, e.g. js/site.js => js.site.js

            var contentBytes = getResourceBytes(resourcePath);

            return contentBytes != null ? new StaticContent(contentBytes, StringExtensions.GetContentTypeByResourceName(resourcePath)) : null;
        }

        private byte[] getResourceBytes(string resourcePath)
        {
            Lazy<byte[]> resource;
            return m_ResourceCache.TryGetValue(resourcePath, out resource) ? resource.Value : null;
        }       

        private static IDictionary<string, Lazy<byte[]>> initializeResourceCache(Assembly assembly, string nsRoot)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (nsRoot == null) throw new ArgumentNullException("nsRoot");

            return assembly.GetManifestResourceNames()
                           .Where(rn => rn.StartsWith(nsRoot, StringComparison.InvariantCultureIgnoreCase))
                           .ToDictionary(
                               rn => rn.Replace(nsRoot, ""),
                               rn => new Lazy<byte[]>(() => assembly.GetManifestResourceStream(rn).ReadAll()), StringComparer.InvariantCultureIgnoreCase);
        }
    }
}