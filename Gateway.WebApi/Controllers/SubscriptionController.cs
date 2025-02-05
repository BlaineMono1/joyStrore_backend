using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.SubscriptionsQuery.Dto;
namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class SubscriptionController
    {
        private readonly SubscriptionsQuerys _subscriptoinsQuerys;

        /// <summary>
        /// Получение списка подписок на главной странице
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult<List<SubscriptionsListDto>>> GetNewsList()
        {
            return await _subscriptoinsQuerys.GetSubscriptionsList();
        }

    }
}
