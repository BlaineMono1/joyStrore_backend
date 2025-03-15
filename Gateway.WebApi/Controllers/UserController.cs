using Gateway.WebApi.Attributes;
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
        public async Task<ActionResult<UserDto>> GetUserByTgId(string tgId)
        {
            try
            {
                _logger.LogInformation("Fetching User with tg ID : {id}", tgId);
                var user = await _usersQuery.UserByTgId(tgId);
                return Ok(user);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Error occurred while Fetching User with tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching user");
            }

        }

        /// <summary>
        /// Вывод истории покупок пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<ActionResult<List<OrderDto>>> GetUserHistoryOrders(string tgId)
        {
            try
            {
                _logger.LogInformation("Fetching user order history with tg ID {id}", tgId);
                var history = await _usersQuery.UserOrder(tgId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Fetching user order historyh tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching user order history");
            }
        }
        
        /// <summary>
        /// Обновление консоли пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> UpdateUserConsole(string tgId, string Console)
        {
            try
            {
                _logger.LogInformation("Updating users console with tg id {tgid}", tgId);
                await _usersQuery.UpdateConsoleType(tgId, Console);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Updating users console with tg id {tgid}", tgId);
                return StatusCode(500, "Error occurred while Updating users console");
            }
        }


        [HttpPost]
        public ActionResult UpdateUserRegion(string region)
        {
            try
            {
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("region", region as String);
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserSettings(Guid userId, string email, string password, string code)
        {
            try
            {
                await _usersQuery.UpdateUserSettings(userId, email, password, code);
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}
