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
                _logger.LogInformation(ex, "Error occurred while Fetching User with tg ID : {id}", tgId);
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
                _logger.LogInformation(ex, "Error occurred while Fetching User cart with tg ID : {id}", tgId);
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
                _logger.LogInformation(ex, "Error occurred while Fetching favorite items with tg ID : {id}", tgId);
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
                _logger.LogInformation(ex, "Error occurred while Fetching user order historyh tg ID : {id}", tgId);
                return StatusCode(500, "Error occurred while fetching user order history");
            }
        }
    }
}
