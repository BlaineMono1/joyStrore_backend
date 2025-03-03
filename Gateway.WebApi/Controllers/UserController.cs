using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.UserQuery;
using Service.Application.Service.UserQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UsersQuery _usersQuery;
        private readonly ILogger<UsersQuery> _logger;

        public UserController(UsersQuery usersQuery, ILogger<UsersQuery> logger)
        {
            _usersQuery = usersQuery;
            _logger = logger;
        }

        /// <summary>
        /// Вывод пользователя
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUserByTgId(string tgId)
        {
            try
            {
                _logger.LogInformation("Fetching User with tg ID : {id}", tgId);
                var user = await _usersQuery.UserByTgId(tgId);
                return Ok(user);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Error occurred while Fetching User with tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching user");
            }

        }

        /// <summary>
        /// Вывод корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<ActionResult<CartDto>> GetUserCart(string tgId)
        {
            try
            {
                _logger.LogInformation("Fetching user cart with tg id : {ID}", tgId);
                var cart = await _usersQuery.UserCart(tgId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Fetching User cart with tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching user cart");
            }
        }
            

        /// <summary>
        /// Вывод избранного пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<ActionResult<List<FavoriteDto>>> GetUserFavorite(string tgId)
        {
            try
            {
                _logger.LogInformation("Fetching user favorite items whith tg ID {id}", tgId);
                var fav = await _usersQuery.UserFavorite(tgId);
                return Ok(fav);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Fetching favorite items with tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching favorite items");
            }
            
        }

        /// <summary>
        /// Вывод истории покупок пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<ActionResult<List<OrderDto>>> GetUserHistoryOrders(string tgId)
        {
            try
            {
                _logger.LogInformation("Fetching user order history with tg ID {id}", tgId);
                var history = await _usersQuery.UserOrder(tgId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Fetching user order historyh tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching user order history");
            }
        }

        /// <summary>
        /// Добавление предмета в корзину пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> AddItemInCart(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation("Adding item with GUID {id} to user cart with tg id {tgid}", itemId, tgId);
                await _usersQuery.UpdateUserCart(tgId, itemId);
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item in user cart with tg ID : {id}, item GUID {id}", tgId, itemId);
                return StatusCode(500, "Error occurred while adding item in user cart");
            }
        }

        /// <summary>
        /// Добавление предмета в избранное пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> AddItemInFavorite(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation("Adding item with GUID {id} to user Favorite with tg id {tgid}", itemId, tgId);
                await _usersQuery.UpdateUserFavorites(tgId, itemId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item in user Favorites with tg ID : {id}, item GUID {id}", tgId, itemId);
                return StatusCode(500, "Error occurred while adding item in user Favorites");
            }
        }

        /// <summary>
        /// Удаление предмета из избранного пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpDelete]

        public async Task<ActionResult> DeleteItemInFavorite(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation("Deleting item with GUID {id} to user Favorite with tg id {tgid}", itemId, tgId);
                await _usersQuery.DeleteFromFavorites(tgId, itemId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Deliting item in user Favorites with tg ID : {id}, item GUID {id}", tgId, itemId);
                return StatusCode(500, "Error occurred while Deliting item in user Favorites");
            }
        }

        /// <summary>
        /// Удаление предмета из корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpDelete]

        public async Task<ActionResult> DeleteFromCart(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation("Deleting item with GUID {id} to user Cart with tg id {tgid}", itemId, tgId);
                await _usersQuery.DeleteFromCart(tgId, itemId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Deliting item in user Cart with tg ID : {id}, item GUID {id}", tgId, itemId);
                return StatusCode(500, "Error occurred while Deliting item in user Cart");
            }
        }

        /// <summary>
        /// Обновление консоли пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> UpdateUserConsole(string tgId, string Console)
        {
            try
            {
                _logger.LogInformation("Updating users console with tg id {tgid}", tgId);
                await _usersQuery.UpdateConsoleType(tgId, Console);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Updating users console with tg id {tgid}", tgId);
                return StatusCode(500, "Error occurred while Updating users console");
            }
        }
    }
}
