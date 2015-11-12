using System.Web.Http;
using Inceptum.WebApi.Help.Description;

namespace Sandbox.Controllers
{
    /// <summary>
    /// Test controller
    /// </summary>
    /// <summary lang="ru">
    /// Тестовый контроллер
    /// </summary>
    /// <remarks>Test controller description</remarks>
    /// <remarks lang="ru">Описание для тестового контроллера</remarks>
    [ApiExplorerOrder(Order = 1)]
    public class TestController : ApiController
    {
        /// <summary>
        /// Test get method
        /// </summary>        
        /// <summary lang="ru">
        /// Тестовый GET метод
        /// </summary>        
        [HttpGet, Route("api/v2/test")]
        public User Get()
        {
            return new User();
        }
    }
}