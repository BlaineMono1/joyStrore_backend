using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
using Service.Application.Service.UserQuery;
using Service.Application.Service.UserQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("user")]
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
        [HttpGet("current")]
        public async Task<ActionResult<UserDto>> GetUserByTgId()
        {
            try
            {
                var user = await _usersQuery.UserByTgId();
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// Вывод истории покупок пользователя
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("purchase-history")]
        public async Task<ActionResult<List<OrderDto>>> GetUserHistoryOrders()
        {
            try
            {
                
                var history = await _usersQuery.UserOrder();
                return Ok(history);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Обновление консоли пользователя
        /// </summary>
        /// <param name="Console"></param>
        /// <returns></returns>
        /// 
        [HttpPut("ps-setting")]
        public async Task<ActionResult> UpdateUserConsole(string Console)
        {
            try
            {
                await _usersQuery.UpdateConsoleType(Console);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Обновление региона для пользователя
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        [HttpPost("region")]
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
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// обновление настроек пользователя 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPut("setting")]
        public async Task<ActionResult> UpdateUserSettings(string email, string password, string? code = null)
        {
            try
            {
                await _usersQuery.UpdateUserSettings(email, password, code);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        /// <summary>
        /// Принимаемый tg userId 
        /// </summary>
        /// <param name="tgId"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ActionResult> UpdateUserTgId(string tgId)
        {
            try
            {
                var options = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("tgId", tgId, options);
                await _usersQuery.CreateUser(tgId);
                return Ok();
            }
            catch (ForbiddenExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Баланс Joy и Joy+ пользователя
        /// </summary>
        /// 

        [HttpGet("balance")]

        public async Task<ActionResult<BalDto>> GetUserBal()
        {
            try
            {
                var reuslt = await _usersQuery.UserBalance();
                return Ok(reuslt);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

    }
}
