using Auth.WebApi.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.UserQuery;
using Service.Application.Service.UserQuery.Dto;

namespace Auth.WebApi.Controllers
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

        [HttpGet("GetBannedUsers")]
        public async Task<ActionResult<List<BlackListDto>>> GetBannedUser()
        {
            try
            {
                var result = await _usersQuery.GetBannedUsers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPut("AddToBlackList")]
        public async Task<ActionResult> AddToBlackList(string tgId)
        {
            try
            {
                await _usersQuery.AddToBlackList(tgId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPut("DeleteFromBlackList")]
        public async Task<ActionResult> DeleteFromBlackList(string tgId)
        {
            try
            {
                await _usersQuery.DeleteFromBlackList(tgId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

    }
}
