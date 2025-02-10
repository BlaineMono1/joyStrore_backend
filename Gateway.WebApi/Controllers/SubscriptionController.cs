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
        public async Task<ActionResult<List<SubscriptionsListDto>>> GetSubscriptionList()
        {
            return await _subscriptoinsQuerys.GetSubscriptionsList();
        }

        /// <summary>
        /// Получение подписки
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(Guid SubscriptionId)
        {
            return await _subscriptoinsQuerys.SubscriptionById(SubscriptionId);
        }

    }
}
