using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.CartQuery;
using Service.Application.Service.CartQuery.Dto;
using Service.Application.Service.UserQuery;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
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

        [HttpGet]

        public async Task<ActionResult<CartDto>> GetUserCart()
        {
            try
            {
                var cart = await _cartQuery.UserCart();
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Добавление предмета в корзину пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> AddItemInCart(Guid productId)
        {
            try
            {
               
                await _cartQuery.UpdateUserCart(productId);
                return Ok();
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "Error occurred while adding item in user cart");
            }
        }

        /// <summary>
        /// Удаление предмета из корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpDelete]

        public async Task<ActionResult> DeleteFromCart(Guid productId)
        {
            try
            {
                await _cartQuery.DeleteFromCart(productId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while Deliting item in user Cart");
            }
        }
    }
}
