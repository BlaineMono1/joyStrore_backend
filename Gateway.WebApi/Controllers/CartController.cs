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

        public async Task<ActionResult<CartDto>> GetUserCart(Guid userId)
        {
            try
            {
                var cart = await _cartQuery.UserCart(userId);
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

        public async Task<ActionResult> AddItemInCart(Guid userId, Guid productId)
        {
            try
            {
                _logger.LogInformation("Adding item with GUID {id} to user cart with tg id {tgid}", productId, userId);
                await _cartQuery.UpdateUserCart(userId, productId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item in user cart with tg ID : {id}, item GUID {id}", userId, productId);
                return StatusCode(500, "Error occurred while adding item in user cart");
            }
        }

        /// <summary>
        /// Удаление предмета из корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpDelete]

        public async Task<ActionResult> DeleteFromCart(Guid userId, Guid productId)
        {
            try
            {
                _logger.LogInformation("Deleting item with GUID {id} to user Cart with tg id {tgid}", productId, userId);
                await _cartQuery.DeleteFromCart(userId, productId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Deliting item in user Cart with tg ID : {id}, item GUID {id}", userId, productId);
                return StatusCode(500, "Error occurred while Deliting item in user Cart");
            }
        }
    }
}
