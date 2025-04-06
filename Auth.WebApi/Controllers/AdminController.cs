using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AdminsQuery;
using Service.Application.Service.AdminsQuery.Dto;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("Admin")]
    public class AdminController :ControllerBase
    {
        private readonly AdminsQuery _query;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminsQuery query, ILogger<AdminController> logger)
        {
            _logger = logger;
            _query = query;
        }

        [HttpGet("GetAdminsList")]
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

        [HttpGet("GetRolesList")]
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

        [HttpGet("CreateAdmin")]
        public async Task<ActionResult> CreateAdmin(string Login, string Password, Guid RoleID)
        {
            try
            {
                await _query.CreateAdmin(Login, Password, RoleID);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ShowAdminRole")]
        public async Task<ActionResult<string>> ShowAdminRole(Guid AdminId)
        {
            try
            {
                var result = await _query.ShowAdminRole(AdminId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdateAdmin")]
        public async Task<ActionResult<string>> UpdateAdmin(Guid AdminId, Guid RoleId)
        {
            try
            {
                await _query.UpdateAdmin(AdminId, RoleId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteAdmin")]
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
