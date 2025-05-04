using Auth.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.UserQuery;
using Service.Application.Service.UserQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

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
        /// <summary>
        /// Вывод списка заблокированных пользователь в админ панели
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("get-banned-users")]
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
        /// <summary>
        /// Добавить пользователя в спискок заблокированных пользователей в админ панели
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("add-to-balcklist")]
        public async Task<ActionResult> AddToBlackList(string tgId)
        {
            try
            {
                await _usersQuery.AddToBlackList(tgId);
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
        /// Удалить пользователя из списка заблокированных пользователей в админ панели
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("delete-from-blacklist")]
        public async Task<ActionResult> DeleteFromBlackList(string tgId)
        {
            try
            {
                await _usersQuery.DeleteFromBlackList(tgId);
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

    }
}
