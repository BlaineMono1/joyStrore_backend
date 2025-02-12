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
    }
}
