using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.MarkUpQuery;
using Service.Application.Service.MarkUpQuery.Dto;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("MarkUp")]
    public class MarkUpController : ControllerBase
    {
        private readonly MarkUpQUery _query;
        private readonly ILogger<MarkUpController> _logger;
        

        public MarkUpController(ILogger<MarkUpController> logger, MarkUpQUery query)
        {
            _query = query;
            _logger = logger;
        }
        [HttpGet("GetMarkUpsProduct")]
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

        [HttpPut("UpdateMarkUpProductPercent")]
        public async Task<ActionResult> UpdateMarkUp(Guid MarkUpId, decimal Percent)
        {
            try
            {
                await _query.UpdatePercetProduct(MarkUpId, Percent);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetMarkUpsSub")]
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

        [HttpPut("UpdateMarkUpSubPercent")]
        public async Task<ActionResult> UpdateMarkUpSub(Guid MarkUpId, decimal Percent)
        {
            try
            {
                await _query.UpdatePercentSub(MarkUpId, Percent);
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
