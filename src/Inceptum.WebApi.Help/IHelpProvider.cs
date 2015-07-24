using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// An interface which provides a help page model.
    /// </summary>
    public interface IHelpProvider
    {
        HelpPageModel GetHelp();
    }

    public interface IPdfTemplateProvider
    {
        Paragraph GetTemplate(string name, object data, BaseFont defaultFont);
    }
}