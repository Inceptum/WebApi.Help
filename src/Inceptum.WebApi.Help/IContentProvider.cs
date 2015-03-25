using System;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// An interface which serves a static content for help pages infrastructure
    /// </summary>
    public interface IContentProvider
    {
        StaticContent GetContent(string resourcePath);
    }

    /// <summary>
    /// Static content annotated with content type
    /// </summary>
    public sealed class StaticContent
    {
        public readonly byte[] ContentBytes;
        public readonly string ContentType;

        public StaticContent(byte[] contentBytes, string contentType)
        {
            if (contentBytes == null) throw new ArgumentNullException("contentBytes");
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentNullException("contentType");

            ContentBytes = contentBytes;
            ContentType = contentType;
        }
    }
}