using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.OrderQuery;
using Service.Application.Service.OrderQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("Orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderQuery _query;

        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderQuery query, ILogger<OrderController> logger)
        {
            _query = query;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<OrdersDto>> CreateOrder(bool IsTokenPayment)
        {
            try
            {
                var result = await _query.CreateOrder(IsTokenPayment);
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
