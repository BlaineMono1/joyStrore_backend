using Microsoft.AspNetCore.Mvc;
using Auth.WebApi.Attributes;
using Business.Data.Models;
using Service.Application.Service.AutahQuery;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("Auth")]
    public class AutarizationController : ControllerBase
    {
        private readonly AutahQuery _query;

        private readonly ILogger<AutarizationController> _logger;


        public AutarizationController(AutahQuery query, ILogger<AutarizationController> logger)
        {
            _query = query;
            _logger = logger;
        }

        [SetRoute("LogIn")]
        [HttpGet]
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
                return StatusCode(500, "Invalid login or password");
            }
        }

        [SetRoute("LogInByToken")]
        [HttpGet]
        public async Task<ActionResult> LogInByToken(string? Token = null)
        {
            try
            {
                var result = await _query.LogInByToken(Token);
                if(string.IsNullOrEmpty(result)) return StatusCode(500, "Token null or expired");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Token null or expired");
            }
        }
    }
}
