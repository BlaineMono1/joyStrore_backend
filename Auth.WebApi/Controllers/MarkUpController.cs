using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
using Service.Application.Service.MarkUpQuery;
using Service.Application.Service.MarkUpQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("mark-up")]
    public class MarkUpController : ControllerBase
    {
        private readonly MarkUpQUery _query;
        private readonly ILogger<MarkUpController> _logger;
        

        public MarkUpController(ILogger<MarkUpController> logger, MarkUpQUery query)
        {
            _query = query;
            _logger = logger;
        }
        /// <summary>
        /// Спиоск наценок на игры и аддоны в адимн панеле
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-mark-up-products")]
        public async Task<ActionResult<List<PercentDto>>> GetMarkUpsProduct()
        {
            try
            {
                var result = await _query.GetMarkUpsProductList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Обновление наценки на игры и аддоны
        /// </summary>
        /// <param name="MarkUpId"></param>
        /// <param name="Percent"></param>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-mark-up-percent")]
        public async Task<ActionResult> UpdateMarkUp(Guid MarkUpId, decimal Percent)
        {
            try
            {
                await _query.UpdatePercetProduct(MarkUpId, Percent);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Спиоск наценок на подписки в адимн панеле
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-mark-up-sub")]
        public async Task<ActionResult<List<PercentSubDto>>> GetMarkUpsSub()
        {
            try
            {
                var result = await _query.GetMarkUpsSubList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Обновление наценки на подписки
        /// </summary>
        /// <param name="MarkUpId"></param>
        /// <param name="Percent"></param>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-mark-up-sub")]
        public async Task<ActionResult> UpdateMarkUpSub(Guid MarkUpId, decimal Percent)
        {
            try
            {
                await _query.UpdatePercentSub(MarkUpId, Percent);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
