using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Inceptum.WebApi.Help.Resources;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// This class handles all requests to web api help page
    /// </summary>
    sealed class HelpPageHandler : DelegatingHandler
    {
        private const string COOKIE_NAME = "help-page";        
        private readonly IContentProvider m_ContentProvider;
        private readonly IHelpProvider m_HelpProvider;
        private readonly List<Tuple<IPdfTemplateProvider, int>> m_PdfTemplateProviders = new List<Tuple<IPdfTemplateProvider, int>>();
        private readonly BaseFont m_ArialFont;
        internal const string HELP_PAGE_ROUTE_NAME = "WebAPIHelpPageRoute";
        internal const string DEFAULT_HELP_PAGE_ROUTE_TEMPLATE = "help/{*resource}";
        internal const string RESOURCE_PARAMETER_NAME = "resource";
        
        public HelpPageHandler(IHelpProvider helpProvider, IContentProvider contentProvider, IEnumerable<Tuple<IPdfTemplateProvider, int>> pdfTemplateProviders)
        {
            if (contentProvider == null) throw new ArgumentNullException("contentProvider");
            if (helpProvider == null) throw new ArgumentNullException("helpProvider");
            m_ContentProvider = contentProvider;
            m_HelpProvider = helpProvider;
            m_PdfTemplateProviders = new List<Tuple<IPdfTemplateProvider, int>>(pdfTemplateProviders.OrderBy(p => p.Item2));
            m_ArialFont = BaseFont.CreateFont("arial.ttf", BaseFont.IDENTITY_H, true, true, Fonts.arial, null, true);        
            m_ArialFont.Subset = true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {            
            setupCulture(request);

            var resourceName = getResourceName(request);

            // When requested at help root address,  e.g. /api/help.
            // Redirect to index.html, for the relative pathes to work properly.
            if (resourceName == null)
            {
                var redirect = request.CreateResponse(HttpStatusCode.Redirect);
                var redirectPath = request.GetRequestContext().Url.Route(HELP_PAGE_ROUTE_NAME, new Dictionary<string, object>() { { RESOURCE_PARAMETER_NAME, "index.html" } });
                redirect.Headers.Location = new Uri(redirectPath, UriKind.Relative);
                return Task.FromResult(redirect);                
            }

            switch (resourceName)
            {
                // api/help/pdf => pdf
                case "pdf":
                    var pdfContent = createHelpPdf();
                    var pdfResponse = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(pdfContent)
                    };
                    pdfResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    return Task.FromResult(pdfResponse);

                // api/help/metadata => metadata JSON
                case "metadata":
                    var metadata = m_HelpProvider.GetHelp();
                    return Task.FromResult(request.CreateResponse(HttpStatusCode.OK, metadata, "application/json"));

                // api/help/index.html, /api/help/css/site.css, /api/help/js/site.js etc.
                default:
                    var staticContent = m_ContentProvider.GetContent(resourceName);
                    if (staticContent != null)
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(staticContent.ContentBytes)
                        };
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue(staticContent.ContentType);

                        //TODO[tv]: Add caching ?

                        return Task.FromResult(response);
                    }
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(string.Format("The resource at {0} was not found", request.RequestUri)) });
            }

        }

        private byte[] createHelpPdf()
        {
            var stream = new MemoryStream();

            var document = new Document(PageSize.A4, 30, 30, 30, 30);

            using (PdfWriter.GetInstance(document, stream))
            {
                document.Open();
                var help = m_HelpProvider.GetHelp();
                bool first = true;
                foreach (var chapter in createPdfSections(help, help.TableOfContent.Children, null, 1))
                {
                    if (chapter is Chapter && !first)
                    {
                        document.NewPage();
                    }
                    document.Add(chapter);
                    first = false;
                }
                document.Close();
            }

            return stream.ToArray();
        }

        private IEnumerable<Section> createPdfSections(HelpPageModel help, IEnumerable<TableOfContentItem> tocItems, Section parent, int level)
        {
            var sections = new List<Section>();
            foreach (var tocItem in tocItems)
            {
                var item = help.Items.FirstOrDefault(i => i.TableOfContentId == tocItem.ReferenceId);
                var title = new Paragraph(tocItem.Text, new Font(m_ArialFont, 12));
                var section = level == 1 ? new Chapter(title, level) : parent.AddSection(20f, title, level);

                if (item != null)
                {
                    if (item.Data != null && item.Template != null)
                    {
                        var paragraph = m_PdfTemplateProviders.Select(x => x.Item1.GetTemplate(item.Template, item.Data, m_ArialFont)).FirstOrDefault(p => p != null);
                        if (paragraph != null)
                        {
                            section.Add(paragraph);
                        }
                    }
                }
                createPdfSections(help, tocItem.Children, section, level + 1);
                sections.Add(section);
            }
            return sections;
        }

        private static void setupCulture(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var ci = CultureInfo.GetCultureInfo("ru-RU"); // Default language is Russian
            var cookie = request.Headers.GetCookies(COOKIE_NAME).FirstOrDefault();

            if (cookie != null && !string.IsNullOrWhiteSpace(cookie[COOKIE_NAME].Value))
            {
                try
                {
                    ci = CultureInfo.GetCultureInfo(cookie[COOKIE_NAME].Value);
                }
                catch (CultureNotFoundException) { /* Ignore */ }
            }

            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = ci;
            Strings.Culture = ci;
        }

        private static string getResourceName(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var resourceName = request.GetRouteData().Values["resource"];

            return resourceName != null ? resourceName.ToString() : null;         
        }
    }
}