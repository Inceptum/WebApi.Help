namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// An interface which provides a help page model.
    /// </summary>
    public interface IHelpProvider
    {
        HelpPageModel GetHelp();
    }
}