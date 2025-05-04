using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.NewsQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("news")]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> _logger;

        private readonly NewsQuery _query;

        public NewsController(ILogger<NewsController> logger, NewsQuery query)
        {
            _logger = logger;
            _query = query;
        }
        /// <summary>
        /// Спиоск новостей в адимн панеле
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("news-list")]
        public async Task<ActionResult<List<NewsListDto>>> GetNewsList()
        {
            try
            {
                var result = await _query.GetNewsListAdminPanel();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Создание новости
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Url"></param>
        /// <param name="Image"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("create-news")]
        public async Task<ActionResult> CreateNews(string Name, string Url, string Image)
        {
            try
            {
                await _query.CreateNews(Name, Url, Image);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Удаление новости
        /// </summary>
        /// <param name="NewsId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("delete-news")]
        public async Task<ActionResult> DeleteNews(Guid NewsId)
        {
            try
            {
                await _query.DeleteNews(NewsId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Обновление новости
        /// </summary>
        /// <param name="NewsId"></param>
        /// <param name="Name"></param>
        /// <param name="Url"></param>
        /// <param name="Image"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("update-news")]
        public async Task<ActionResult> UpdateNews(Guid NewsId, string? Name = null, string? Url = null, string? Image = null)
        {
            try
            {
                await _query.UpdateNews(NewsId, Name, Url, Image);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Получение новости по id
        /// </summary>
        /// <param name="NewsId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("get-news-by-id")]
        public async Task<ActionResult<NewsListDto>> GetNewsById(Guid NewsId)
        {
            try
            {
                var result = await _query.GetNewsById(NewsId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
