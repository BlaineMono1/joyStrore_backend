using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.UserQuery;
using Service.Application.Service.UserQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class UserController
    {
        private readonly UsersQuery _usersQuery;


        /// <summary>
        /// Вывод пользователя
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<UserDto> GetUserByTgId(string tgId)
        {
            return await _usersQuery.UserByTgId(tgId);
        }

        /// <summary>
        /// Вывод корзины пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<CartDto> GetUserCart(string tgId)
        {
            return await _usersQuery.UserCart(tgId);
        }

        /// <summary>
        /// Вывод избранного пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<List<FavoriteDto>> GetUserFavorite(string tgId)
        {
            return await _usersQuery.UserFavorite(tgId);
        }

        /// <summary>
        /// Вывод истории покупок пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]

        public async Task<List<OrderDto>> GetUserHistoryOrders(string tgId)
        {
            return await _usersQuery.UserOrder(tgId);
        }
    }
}
