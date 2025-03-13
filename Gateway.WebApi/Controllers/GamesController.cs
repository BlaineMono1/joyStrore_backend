using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.Application.Extension.Pagination;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.GamesQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly GamesQuery _gamesQuery;
        private readonly ILogger<GamesController> _logger;

        public GamesController(GamesQuery gamesQuery, ILogger<GamesController> logger, IProductRepository<Product> productRepository)
        {
            _gamesQuery = gamesQuery;
            _logger = logger;
            _productRepository = productRepository;
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
        [HttpPost]
        public async Task<ActionResult<List<GamesListDto>>> FilterGames(string? name = null, List<string>? geners = null, int Page = 0)
        {
            try
            {
                _logger.LogInformation("Filtering games");
                var games = await _productRepository.FilterProducts(name, geners);

                var result = await _gamesQuery.FilteredGamesList(new PaginatedList<Product>(games, Page).Entities);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while filtering games");
                return StatusCode(500, "An error occurred while filtering games.");
            }
        }
    }
}
