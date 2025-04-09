using Auth.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
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
        public UserController(UsersQuery usersQuery, ILogger<UsersQuery> logger)
        {
            _usersQuery = usersQuery;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Worker")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
