using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.SubscriptionsQuery.Dto;



namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("SubAdmin")]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionsQuerys _query;

        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(SubscriptionsQuerys query, ILogger<SubscriptionController> logger)
        {
            _query = query;
            _logger = logger;
        }

        [HttpGet("GetPriceSubList")]
        public async Task<ActionResult<List<PriceSubDto>>> GetPricesSubList()
        {
            try
            {
                var result = await _query.GetPriceSubList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdatePriceSub")]
        public async Task<ActionResult> UpdatePriceSub(Guid SubId, decimal Price, string Region)
        {
            try
            {
               await _query.UpdateSubPrice(SubId, Price, Region);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetDiscountSubList")]
        public async Task<ActionResult<List<DiscountSubDto>>> GetDiscountsSubList()
        {
            try
            {
                var result = await _query.GetDiscountSubList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdateDiscountSub")]
        public async Task<ActionResult> UpdateDiscountsSub(Guid SubId, string Percent)
        {
            try
            {
                await _query.UpdateSubDiscount(SubId, Percent);
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
