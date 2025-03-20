using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.Application.Extension.Pagination;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.GamesQuery.Dto;
using Service.Application.Service.ProductQuery.Dto;

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
                
    }
}
