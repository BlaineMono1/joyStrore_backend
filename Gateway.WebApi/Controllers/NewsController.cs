using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.GetNewsList.Dto;
namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class NewsController
    {
        private readonly NewsQuery _newsQuery;
        /// <summary>
        /// Получение новостника
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<NewsDto>>> GetNewsList()
        {
            return await _newsQuery.GetNewsList();
        }
    }
}
