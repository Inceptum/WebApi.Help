using System;
using System.Threading;
using Inceptum.WebApi.Help.Extensions;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Content provider with localization capability.
    /// </summary>
    public class LocalizableContentProvider : IContentProvider
    {
        private readonly IContentProvider m_Inner;

        public LocalizableContentProvider(IContentProvider inner)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            m_Inner = inner;
        }

        public StaticContent GetContent(string resourcePath)
        {
            if (resourcePath == null) throw new ArgumentNullException("resourcePath");

            var contentType = StringExtensions.GetContentTypeByResourceName(resourcePath);

            if (CanLocalize(contentType))
            {
                var content = m_Inner.GetContent(GetLocalizedPath(resourcePath));
                if (content != null) return content;
            }

            return m_Inner.GetContent(resourcePath);
        }

        protected virtual bool CanLocalize(string contentType)
        {
            if (contentType == null) throw new ArgumentNullException("contentType");

            return string.Equals(contentType, "text/html", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual string GetLocalizedPath(string originalResourcePath)
        {
            if (originalResourcePath == null) throw new ArgumentNullException("originalResourcePath");

            var idxLastDot = originalResourcePath.LastIndexOf('.');

            if (idxLastDot == -1) return originalResourcePath;

            return originalResourcePath.Substring(0, idxLastDot + 1)
                   + Thread.CurrentThread.CurrentUICulture.Name.Replace("-", "")
                   + originalResourcePath.Substring(idxLastDot);
        }
    }
}