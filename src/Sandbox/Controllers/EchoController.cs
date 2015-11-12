using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using Inceptum.WebApi.Help.Description;

namespace Sandbox.Controllers
{
    /// <summary>
    /// Echo controller
    /// </summary>   
    /// <summary lang="ru">
    /// Эхо-контроллер
    /// </summary>   
    /// <remarks>Simple echo and user controller</remarks>
    /// <remarks lang="ru">Простой контроллер эха и пользователей</remarks>
    [ApiExplorerOrder(Order = 5)]
    public class EchoController : ApiController
    {
        /// <summary>Echo service.</summary>      
        /// <summary lang="ru">Эхо-сервис.</summary> 
        /// <param name="str">A string to be repeated.</param>
        /// <param name="str" lang="ru">Строка для повторения.</param>        
        /// <param name="times">A number of times to repeat the string.</param>
        /// <param name="times" lang="ru">Требуемое количество повторений.</param>
        /// <param name="capitalize">True if the parameter must be capitalized, false - otherwise. False is default value.</param>
        /// <param name="capitalize" lang="ru">True - капитализационировать, false-нет. По-умолчанию - false.</param>
        /// <remarks>Echoes input message given number of times.</remarks>
        /// <remarks lang="ru">Повторяет входящее сообщение заданное количество раз.</remarks>
        [HttpGet]
        public string GetEcho(string str, int times, bool? capitalize = false)
        {
            return Enumerable.Repeat(0, times).Aggregate(new StringBuilder(), (sb, i) => sb.AppendLine(str)).ToString();
        }

        /// <summary>Get user.</summary>        
        /// <summary lang="ru">Получить пользователя.</summary>   
        /// <param name="id">An id of the user.</param>
        /// <param name="id" lang="ru">Идентификатор пользователя.</param>
        /// <returns>A structure with user's data</returns> 
        /// <returns>Структура с данными пользователя</returns> 
        /// <remarks>Returns user info by identifier.</remarks>
        /// <remarks lang="ru">Возвращает информацию о пользователе по его идентификатору.</remarks>
        [HttpGet, Route("api/users/{id:int}")]
        [ApiExplorerOrder(Order = 2)]
        public User GetUser(int? id)
        {
            return new User();
        }

        /// <summary>Get users.</summary>        
        /// <summary lang="ru">Получить пользователей.</summary>           
        /// <returns>A paged list of users.</returns> 
        /// <returns>Возвращает пейджированный список пользователей.</returns> 
        /// <remarks>Get a collection of users.</remarks>
        /// <remarks lang="ru">Получить список пользователей.</remarks>
        [HttpGet, Route("api/users"), ResponseType(typeof(PagingResponseModel<User>))]
        [ApiExplorerOrder(Order = 3)]
        public HttpResponseMessage GetUsers()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>Get user's contacts.</summary>
        /// <summary lang="ru">Получить контакты пользователя.</summary>
        /// <param name="id">Id of the user to get contacts for.</param>        
        /// <param name="id" lang="ru">Идентификатор пользователя</param>
        /// <remarks>Returns user's contact information by id of the user.</remarks>
        /// <remarks lang="ru">Возвращает контакты пользователя по его идентификатору.</remarks>
        [HttpGet, Route("api/users/{id:int}/contacts")]
        [ApiExplorerOrder(Order = 5)]
        public Contact GetContacts(int? id)
        {
            return new Contact();
        }

        /// <summary>Create a user.</summary>
        /// <summary lang="ru">Создать пользователя.</summary>
        /// <param name="dto">A dto object with data of the user to be saved.</param>        
        /// <param name="dto" lang="ru">DTO-объект с данными создаваемого пользователя.</param>
        /// <remarks>Creates new user using request body data.</remarks>
        /// <remarks lang="ru">Создает нового пользователя, используя данные из тела запроса.</remarks>
        [HttpPost, ResponseType(typeof(User)), Route("api/users")]
        [ApiExplorerOrder(Order = 1)]
        public HttpResponseMessage CreateUser(User dto)
        {
            return Request.CreateResponse(HttpStatusCode.Created, dto);
        }
    }
}