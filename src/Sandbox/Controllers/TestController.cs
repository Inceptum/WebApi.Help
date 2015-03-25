using System.Web.Http;

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