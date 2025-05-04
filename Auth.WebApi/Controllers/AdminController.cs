using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AdminsQuery;
using Service.Application.Service.AdminsQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("admins")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminsQuery _query;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminsQuery query, ILogger<AdminController> logger)
        {
            _logger = logger;
            _query = query;
        }

        /// <summary>
        /// Список всех админов и воркеров
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-admin-list")]
        public async Task<ActionResult<List<AdminListDto>>> GetAdminsList()
        {
            try
            {
                var result = await _query.AdminsList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Выпадающий список ролей
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-roles-list")]
        public async Task<ActionResult<List<RolesList>>> GetRolesList()
        {
            try
            {
                var result = await _query.RolesList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Создание нового админа
        /// </summary>
        /// <param name="Login"></param>
        /// <param name="Password"></param>
        /// <param name="RoleID"></param>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpGet("create-admin")]
        public async Task<ActionResult> CreateAdmin(string Login, string Password, Guid RoleID)
        {
            try
            {
                await _query.CreateAdmin(Login, Password, RoleID);
                return Ok();
            }
            catch(NotFoundException ex)
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
        /// Роль админа
        /// </summary>
        /// <param name="AdminId"></param>
        [Authorize(Roles = "Admin")]
        [HttpGet("show-admin-role")]
        public async Task<ActionResult<string>> ShowAdminRole(Guid AdminId)
        {
            try
            {
                var result = await _query.ShowAdminRole(AdminId);
                return Ok(result);
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
        /// Обновить роль админа 
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="RoleId"></param>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-admin")]
        public async Task<ActionResult<string>> UpdateAdmin(Guid AdminId, Guid RoleId)
        {
            try
            {
                await _query.UpdateAdmin(AdminId, RoleId);
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
        /// Удалить админа 
        /// </summary>
        /// <param name="AdminId"></param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-admin")]
        public async Task<ActionResult<string>> DeleteAdmin(Guid AdminId)
        {
            try
            {
                await _query.DeleteAdmin(AdminId);
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
