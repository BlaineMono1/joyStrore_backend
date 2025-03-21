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

        [HttpGet]
        public ActionResult<List<int>> GetJoyDonat()
        {
            var result = new JoyesDonsDto();

            return Ok(result.Joy);
        }

        [HttpGet]
        public ActionResult<List<int>> GetJoyPlusDonat()
        {
            var result = new JoyesDonsDto();

            return Ok(result.JoyPlus);
        }
    }
}
