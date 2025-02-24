using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.GamesQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GamesQuery _gamesQuery;
        private readonly ILogger<GamesController> _logger;

        public GamesController(GamesQuery gamesQuery, ILogger<GamesController> logger)
        {
            _gamesQuery = gamesQuery;
            _logger = logger;
        }

        /// <summary>
        /// Вывод списка игр на главной странице
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GamesListDto>>> GetGamesList()
        {
            try
            {
                _logger.LogInformation("Fetching game list.");
                var gamesList = await _gamesQuery.GamesList();
                return Ok(gamesList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching game list.");
                return StatusCode(500, "An error occurred while retrieving the games list.");
            }
        }

        /// <summary>
        /// Вывод игры по id и edition
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<GameDto>> GetGame(Guid GameId, Guid Edition)
        {
            try
            {
                _logger.LogInformation("Fetching game details for GameId: {GameId}, Edition: {Edition}", GameId, Edition);
                var game = await _gamesQuery.ShowGame(GameId, Edition);
                return Ok(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching game details for GameId: {GameId}, Edition: {Edition}", GameId, Edition);
                return StatusCode(500, "An error occurred while retrieving the game details.");
            }
        }


        /// <summary>
        /// Фильтрация игр по названию и жанрам
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GamesListDto>>> FilterGames(string name, List<string> geners)
        {
            try
            {
                _logger.LogInformation("Filtering games");
                var games = await _gamesQuery.FilterGames(name, geners);    
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while filtering games");
                return StatusCode(500, "An error occurred while filtering games.");
            }
        }
    }
}
