using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.OrderQuery;
using Service.Application.Service.OrderQuery.Dto;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("Admin/Orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderQuery _query;

        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderQuery query, ILogger<OrderController> logger)
        {
            _query = query;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpGet]
        public async Task<ActionResult<List<OrderListDto>>> GetOrdersList()
        {
            try
            {
                var result = await _query.OrdersList();
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
