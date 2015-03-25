using System.Collections.Generic;

namespace Inceptum.WebApi.Help.Builders
{
    /// <summary>
    /// A participant of the help bulding process.
    /// </summary>
    public interface IHelpBuilder
    {
        IEnumerable<HelpItem> BuildHelp();
    }
}