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
        /// Вывод списка подписок для обновления цены в админ панели
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
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
        /// <summary>
        /// Обновление цены подписки в админ панели
        /// </summary>
        /// <param name="SubId"></param>
        /// <param name="Price"></param>
        /// <param name="Region"></param>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-sub-price")]
        public async Task<ActionResult> UpdatePriceSub(Guid SubId, decimal Price, string Region)
        {
            try
            {
               await _query.UpdateSubPrice(SubId, Price, Region);
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
        /// Список подписок для обновления процентов в админ панели
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-discound-sub-list")]
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

        /// <summary>
        /// Обновление скидки подписки в админ панели
        /// </summary>
        /// <param name="SubId"></param>
        /// <param name="Percent"></param>
        //[Authorize(Roles = "Admin")]
        //[HttpPut("UpdateDiscountSub")]
        //public async Task<ActionResult> UpdateDiscountsSub(Guid SubId, string Percent)
        //{
        //    try
        //    {
        //        await _query.UpdateSubDiscount(SubId, Percent);
        //        return Ok();
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(404, ex.Message);
        //    }
        //    catch (BadRequestExeption ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(400, ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(500, ex.Message);
        //    }
        //}
    }
}
