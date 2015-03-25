using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// This interface extends capabilities of default <see cref="T:System.Web.Http.Description.IDocumentationProvider"/>.
    /// </summary>
    public interface IExtendedDocumentationProvider : IDocumentationProvider
    {
        /// <summary>
        /// Returns displayable name of the controller.
        /// </summary>                
        string GetName(HttpControllerDescriptor controllerDescriptor);

        /// <summary>
        /// Returns displayable name of the action.
        /// </summary>        
        string GetName(HttpActionDescriptor actionDescriptor);
    }
}