using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
using Service.Application.Service.TransactionQuery;
using Service.Application.Service.TransactionQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionQuery _query;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ILogger<TransactionController> logger, TransactionQuery query)
        {
            _logger = logger;
            _query = query;
        }

        /// <summary>
        /// Вывод joy
        /// </summary>
        /// <returns></returns>
        ///
        [HttpGet("joy")]
        public ActionResult<List<decimal>> GetJoyDonat()
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
        public ActionResult<List<decimal>> GetJoyPlusDonat()
        {
            var result = new JoyesDonsDto();

            return Ok(result.JoyPlus);
        }

        /// <summary>
        /// Количество joy, если пользователь купит joy токены
        /// </summary>
        /// <param name="JoyAmount"></param>
        /// <returns></returns>
        ///
        [HttpGet("new-joy-bal")]
        public async Task<ActionResult<decimal>> GetNewJoyBal(decimal JoyAmount)
        {
            try
            {
                var result = await _query.UserJoyBalAfterrRplenishment(JoyAmount);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, "Data not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Server error");
            }
        }

        /// <summary>
        /// Купить joy за рубли
        /// </summary>
        /// <param name="JoyAmount"></param>
        /// <returns></returns>
        ///
        [HttpGet("buy-joy-rub")]
        public async Task<ActionResult> BuyJoyRub(decimal JoyAmount)
        {
            try
            {
                await _query.BuyJoyRub(JoyAmount);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, "Data not found");
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Server error");
            }
        }

        /// <summary>
        /// Купить joy за joy+
        /// </summary>
        /// <param name="JoyAmount"></param>
        /// <returns></returns>
        ///
        [HttpGet("buy-joy-joy-plus")]
        public async Task<ActionResult> BuyJoyJoyPlus(decimal JoyAmount)
        {
            try
            {
                await _query.BuyJoy(JoyAmount);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, "Data not found");
            }
            catch (BadRequestExeption ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Server error");
            }
        }
    }
}
