using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.SubscriptionsQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;



namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("subscriptions")]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionsQuerys _query;

        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(SubscriptionsQuerys query, ILogger<SubscriptionController> logger)
        {
            _query = query;
            _logger = logger;
        }

        /// <summary>
        /// Список подписок для обновления процентов
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-mark-up-subscriptions-list")]
        public async Task<ActionResult<List<MarkUpSubDto>>> GetMarkUpList()
        {
            try
            {
                var result = await _query.GetMarkUpList();
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

        /// <summary>
        /// Список подписок для обновления процентов
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-mark-up-subscriptions")]
        public async Task<ActionResult<List<MarkUpSubDto>>> UpdatePercent(Guid Id, decimal Percent)
        {
            try
            {
                await _query.UpdatePercent(Id, Percent);
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
