using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.UserQuery;
using Service.Application.Service.UserQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UsersQuery _usersQuery;
        private readonly ILogger<UsersQuery> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserController(UsersQuery usersQuery, ILogger<UsersQuery> logger, IHttpContextAccessor httpContextAccessor)
        {
            _usersQuery = usersQuery;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Вывод пользователя
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUserByTgId()
        {
            try
            {
                var user = await _usersQuery.UserByTgId();
                return Ok(user);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Error occurred while fetching user");
            }

        }

        /// <summary>
        /// Вывод истории покупок пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<ActionResult<List<OrderDto>>> GetUserHistoryOrders()
        {
            try
            {
                
                var history = await _usersQuery.UserOrder();
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Error occurred while fetching user order history");
            }
        }
        
        /// <summary>
        /// Обновление консоли пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> UpdateUserConsole(string Console)
        {
            try
            {
                await _usersQuery.UpdateConsoleType(Console);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Error occurred while Updating users console");
            }
        }


        [HttpPost]
        public ActionResult UpdateUserRegion(string region)
        {
            try
            {
                var options = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("region", region, options);
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserSettings(string email, string password, string code)
        {
            try
            {
                await _usersQuery.UpdateUserSettings(email, password, code);
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateuserTgId(string tgId)
        {
            try
            {
                var options = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("tgId", tgId, options);
                _logger.LogError(_httpContextAccessor.HttpContext?.Request.Cookies["tgId"]);
                await _usersQuery.CreateUser(tgId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex);
            }
        }
    }
}
