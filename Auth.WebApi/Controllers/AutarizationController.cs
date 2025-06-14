using Microsoft.AspNetCore.Mvc;
using Auth.WebApi.Attributes;
using Business.Data.Models;
using Service.Application.Service.AutahQuery;
using Microsoft.AspNetCore.Authorization;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("auth")]
    public class AutarizationController : ControllerBase
    {
        private readonly AutahQuery _query;

        private readonly ILogger<AutarizationController> _logger;


        public AutarizationController(AutahQuery query, ILogger<AutarizationController> logger)
        {
            _query = query;
            _logger = logger;
        }

        /// <summary>
        /// Вход по логину и паролю
        /// </summary>
        /// <param name="Login"></param>
        /// <param name="Password"></param>
        [HttpPost("log-in")]
        public async Task<ActionResult<string>> LogIn(string Login, string Password)
        {
            try
            {
                var result = await _query.LogIn(Login, Password);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(403, ex.Message);
            }
        }

        /// <summary>
        /// Роль по токену
        /// </summary>
        [SetRoute("get-me")]
        [HttpPost]
        public async Task<ActionResult> getMe(string? Token = null)
        {
            try
            {
                var result = await _query.getMe(Token);

                if (result == Guid.Empty)
                {
                    return StatusCode(400, "Invalid Token");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
