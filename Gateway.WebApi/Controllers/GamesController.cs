using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AddOnsQuery.Dto;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.GamesQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class GamesController
    {
        private readonly GamesQuery _gamesQuery;

        /// <summary>
        /// Вывод списка игр на главной странице
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<GamesListDto>>> GetGamesList()
        {
            return await _gamesQuery.GamesList();
        }

        /// <summary>
        /// Вывод игры по id и edition
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<GameDto> GetGame(Guid GameId, string? Edition)
        {
            return await _gamesQuery.ShowGame(GameId, Edition);
        }
    }
}
