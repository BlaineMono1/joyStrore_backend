using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.CartQuery;
using Service.Application.Service.CartQuery.Dto;
using Service.Application.Service.UserQuery;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartQuery _cartQuery;
        private readonly ILogger<CartQuery> _logger;

        public CartController(CartQuery cartQuery, ILogger<CartQuery> logger)
        {
            _cartQuery = cartQuery;
            _logger = logger;
        }


        /// <summary>
        /// Вывод корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("by-user")]
        public async Task<ActionResult<CartDto>> GetUserCart()
        {
            try
            {
                var cart = await _cartQuery.UserCart();
                return Ok(cart);
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
        /// Добавление предмета в корзину пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut("add-item")]
        public async Task<ActionResult> AddItemInCart(Guid productId)
        {
            try
            {
               
                await _cartQuery.UpdateUserCart(productId);
                return Ok();
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
        /// Удаление предмета из корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete("remove-item")]
        public async Task<ActionResult> DeleteFromCart(Guid productId)
        {
            try
            {
                await _cartQuery.DeleteFromCart(productId);
                return Ok();
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
    }
}
