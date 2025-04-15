using Auth.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.TransactionQuery;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("Transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionQuery _query;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(TransactionQuery query, ILogger<TransactionController> logger)
        {
            _query = query;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("plus-joy-balance")]

        public async Task<ActionResult> UpdateUserJoyBalanceInc(string tgId, decimal amount)
        {
            try
            {
                await _query.IncUserJoyBal(tgId, amount);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("minus-joy-balance")]

        public async Task<ActionResult> UpdateUserJoyBalanceDec(string tgId, decimal amount)
        {
            try
            {
                await _query.DecUserJoyBal(tgId, amount);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("plus-joy-plus-balance")]

        public async Task<ActionResult> UpdateUserJoyPlusBalanceInc(string tgId, decimal amount)
        {
            try
            {
                await _query.IncUserJoyPlusBal(tgId, amount);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("minus-joy-plus-balance")]

        public async Task<ActionResult> UpdateUserJoyPlusBalanceDec(string tgId, decimal amount)
        {
            try
            {
                await _query.DecUserJoyPlusBal(tgId, amount);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
