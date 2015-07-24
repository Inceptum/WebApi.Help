using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;

namespace Inceptum.WebApi.Help.Builders
{
    /// <summary>
    /// Adds error codes documentation to help page.
    /// </summary>
    public class ErrorsDocumentationBuilder : IHelpBuilder
    {
        private readonly ResourceManager m_ResourceManager;
        private readonly string m_TocRoot;
        private const string DEFAULT_TOC_PATH = "APIErrors";
        private const string TEMPLATE_NAME = "errorCodes";

        public ErrorsDocumentationBuilder()
            : this(DEFAULT_TOC_PATH)
        {
        }

        public ErrorsDocumentationBuilder(string tocPath)
            : this(new ResourceManager(typeof(Strings)), tocPath)
        {
        }

        public ErrorsDocumentationBuilder(ResourceManager resourceManager, string tocPath = DEFAULT_TOC_PATH)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (string.IsNullOrWhiteSpace(tocPath)) throw new ArgumentNullException("tocPath");
            m_ResourceManager = resourceManager;
            m_TocRoot = (tocPath ?? string.Empty).Trim();
        }

        public IEnumerable<HelpItem> BuildHelp()
        {
            return CreateHelpItems();
        }

        protected virtual IEnumerable<HelpItem> CreateHelpItems()
        {
            yield return new HelpItem(m_TocRoot)
                {
                    Title = Strings.ErrorCodes_Title,
                    Template = TEMPLATE_NAME,
                    Data = GetErrorCodes().ToArray()
                };
        }

        protected virtual IEnumerable<ErrorCodeDto> GetErrorCodes()
        {
            return GetSupportedHttpErrorCodes().Select(err => new ErrorCodeDto()
                {
                    Code = (int)err,
                    Description = m_ResourceManager.GetString(string.Format("ErrorCodes_{0}", (int)err))
                });
        }

        protected virtual IEnumerable<HttpStatusCode> GetSupportedHttpErrorCodes()
        {
            yield return HttpStatusCode.BadRequest;
            yield return HttpStatusCode.Unauthorized;
            yield return HttpStatusCode.Forbidden;
            yield return HttpStatusCode.MethodNotAllowed;
            yield return HttpStatusCode.NotAcceptable;
            yield return HttpStatusCode.Gone;
            yield return (HttpStatusCode)429; // To many requests
            yield return HttpStatusCode.InternalServerError;
            yield return HttpStatusCode.ServiceUnavailable;
        }

        public class ErrorCodeDto
        {
            public int Code { get; set; }
            public string Description { get; set; }
        }
    }
};