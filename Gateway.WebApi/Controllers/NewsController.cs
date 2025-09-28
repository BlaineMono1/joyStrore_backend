using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.GetNewsList.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("news")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsQuery _newsQuery;
        ILogger<NewsController> _logger;

        public NewsController(NewsQuery newsQuery, ILogger<NewsController> logger)
        {
            _newsQuery = newsQuery;
            _logger = logger;
        }

        /// <summary>
        /// Получение новостника
        /// </summary>
        /// <returns></returns>
        [HttpGet("board")]
        public async Task<ActionResult<List<NewsDto>>> GetNewsList()
        {
            try
            {
                _logger.LogInformation("Fetching news list");
                var news = await _newsQuery.GetNewsList();
                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
