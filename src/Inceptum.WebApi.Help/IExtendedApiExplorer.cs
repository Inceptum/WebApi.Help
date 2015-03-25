using System.Collections.Generic;
using System.Web.Http.Description;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Defines an interface for getting a collection of <see cref="T:Inceptum.WebApi.Help.ExtendedApiDescription"/>.
    /// </summary>
    public interface IExtendedApiExplorer : IApiExplorer
    {
        ICollection<ExtendedApiDescription> ExtendedApiDescriptions { get; }
    }
}