using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.TransactionQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ILogger<TransactionController> logger) { _logger = logger; }


        /// <summary>
        /// Вывод joy
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("joy")]
        public ActionResult<List<int>> GetJoyDonat()
        {
            var result = new JoyesDonsDto();

            return Ok(result.Joy);
        }
        /// <summary>
        /// Вывод joy+
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("joy-plus")]
        public ActionResult<List<int>> GetJoyPlusDonat()
        {
            var result = new JoyesDonsDto();

            return Ok(result.JoyPlus);
        }
    }
}
