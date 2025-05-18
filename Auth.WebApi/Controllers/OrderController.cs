using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
using Service.Application.Service.OrderQuery;
using Service.Application.Service.OrderQuery.Dto;
using StackExchange.Redis;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("orders")]
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
        /// Список ордеров адимина\воркера
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("new-orders-list")]
        public async Task<ActionResult<List<OrderListDto>>> WorkerOrders(Guid WorkerId)
        {
            try
            {
                var result = await _query.WorkerOrders(WorkerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }


        /// <summary>
        /// Список не взятых ордеров 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("free-orders-list")]
        public async Task<ActionResult<List<OrderListDto>>> NotTakenOreders()
        {
            try
            {
                var result = await _query.NotTakenOreders();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Взять заказ
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("take-order")]
        public async Task<ActionResult<List<OrderListDto>>> TakeOrder(Guid OrderId, Guid WorkerId)
        {
            try
            {
                await _query.TakeOrder(OrderId, WorkerId);
                return Ok();
            }
            catch(NotFoundException ex)
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
        /// Отказаться от заказа
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("refuse-order")]
        public async Task<ActionResult<List<OrderListDto>>> RefuseOredr(Guid OrderId, Guid WorkerId)
        {
            try
            {
                await _query.RefuseOrder(OrderId, WorkerId);
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
        /// Заказ выполнен
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("finish-order")]
        public async Task<ActionResult<List<OrderListDto>>> OrderDone(Guid OrderId, Guid WorkerId)
        {
            try
            {
                await _query.OrderDone(OrderId, WorkerId);
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
        /// Возврат заказа
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("cancel-order")]
        public async Task<ActionResult<List<OrderListDto>>> CancelOrder(Guid OrderId)
        {
            try
            {
                await _query.CancelOrder(OrderId);
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
        /// Список всех ордеров
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("all-orders-list")]
        public async Task<ActionResult<List<OrderListDto>>> GetAllOrdersList()
        {
            try
            {
                var result =  await _query.GetAllOrdersList();
                return Ok(result);
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
