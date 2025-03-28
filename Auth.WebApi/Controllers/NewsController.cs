using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.NewsQuery.Dto;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("NewsAdmin")]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> _logger;

        private readonly NewsQuery _query;

        public NewsController(ILogger<NewsController> logger, NewsQuery query)
        {
            _logger = logger;
            _query = query;
        }

        [HttpGet("NewsListAdmin")]
        public async Task<ActionResult<List<NewsListDto>>> GetNewsList()
        {
            try
            {
                var result = await _query.GetNewsListAdminPanel();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
