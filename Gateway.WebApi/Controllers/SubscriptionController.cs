using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.SubscriptionsQuery.Dto;
namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionsQuerys _subscriptoinsQuerys;
        private readonly ILogger<SubscriptionController> _logger;
        public SubscriptionController(SubscriptionsQuerys subscriptoinsQuerys, ILogger<SubscriptionController> logger)
        {
            _logger = logger;
            _subscriptoinsQuerys = subscriptoinsQuerys;
        }

        /// <summary>
        /// Получение списка подписок на главной странице
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<ActionResult<List<SubscriptionsListDto>>> GetSubscriptionList()
        {
            try
            {
                _logger.LogInformation("Fetching subscription list");
                var subsList = await _subscriptoinsQuerys.GetSubscriptionsList(); ;
                return Ok(subsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching subscription list.");
                return StatusCode(500, "An error occurred while fetching subscription list.");
            }
            
        }
    }
}
