using Auth.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
using Service.Application.Service.TransactionQuery;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionQuery _query;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(TransactionQuery query, ILogger<TransactionController> logger)
        {
            _query = query;
            _logger = logger;
        }
        /// <summary>
        /// Добавить joy пользователю
        /// </summary>
        /// <param name="tgId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("increase-joy-balance")]

        public async Task<ActionResult> UpdateUserJoyBalanceInc(string tgId, decimal amount)
        {
            try
            {
                await _query.IncUserJoyBal(tgId, amount);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Отнять joy у пользователя
        /// </summary>
        /// <param name="tgId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("decrease-joy-balance")]

        public async Task<ActionResult> UpdateUserJoyBalanceDec(string tgId, decimal amount)
        {
            try
            {
                await _query.DecUserJoyBal(tgId, amount);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }


        /// <summary>
        /// Добавить joy+ пользователю
        /// </summary>
        /// <param name="tgId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("increase-joy-plus-balance")]
        public async Task<ActionResult> UpdateUserJoyPlusBalanceInc(string tgId, decimal amount)
        {
            try
            {
                await _query.IncUserJoyPlusBal(tgId, amount);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Отнять joy+ у пользователя
        /// </summary>
        /// <param name="tgId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("decrease-joy-plus-balance")]

        public async Task<ActionResult> UpdateUserJoyPlusBalanceDec(string tgId, decimal amount)
        {
            try
            {
                await _query.DecUserJoyPlusBal(tgId, amount);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
