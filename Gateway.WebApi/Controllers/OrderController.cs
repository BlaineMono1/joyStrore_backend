using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Extension.Pagination;
using Service.Application.Service.OrderQuery;
using Service.Application.Service.OrderQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("orders")]
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
        /// <summary>
        /// Создание ордера при оплате за рубли
        /// </summary>
        /// <returns></returns>
        [HttpPost("create-order-rub")]
        public async Task<ActionResult<OrdersDto>> CreateOrderRub(string PsEmail, string PsPass, string PsCode, string ReciptEmail, bool isSave)
        {
            try
            {
                await _query.CreateOrderRub(PsEmail, PsPass, PsCode, ReciptEmail, isSave);
                return Ok();
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
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
        /// Создание ордера при оплате за joy
        /// </summary>
        /// <returns></returns>
        [HttpPost("create-order-joy")]
        public async Task<ActionResult<OrdersDto>> CreateOrderJ(string PsEmail, string PsPass, string PsCode, string ReciptEmail, bool isSave)
        {
            try
            {
                await _query.CreateOrderJ(PsEmail, PsPass, PsCode, ReciptEmail, isSave);
                return Ok();
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
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
        /// Список ордеров пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-orders-list")]
        public async Task<ActionResult<List<UserOrdersListDto>>> GetOrdersList(int Page = 0)
        {
            try
            {
                var result = (await _query.GetUserOrldersList()).AsQueryable();
                return Ok(new PaginatedList<UserOrdersListDto>(result, Page).Entities);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
