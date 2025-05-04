using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.SubscriptionsQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;
namespace Gateway.WebApi.Controllers
{
    [SetRoute("")]
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

        [HttpGet("layout")]
        public async Task<ActionResult<List<SubscriptionsListDto>>> GetSubscriptionList()
        {
            try
            {
                _logger.LogInformation("Fetching subscription list");
                var subsList = await _subscriptoinsQuerys.GetSubscriptionsList(); ;
                return Ok(subsList);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
            
        }
    }
}
