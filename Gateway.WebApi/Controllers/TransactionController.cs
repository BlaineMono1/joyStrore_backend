using System.Text;
using System.Text.Json;
using Gateway.WebApi.Attributes;
using Gateway.WebApi.Dto;
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
        private readonly string _apiKey;

        public TransactionController(ILogger<TransactionController> logger, TransactionQuery query)
        {
            _logger = logger;
            _query = query;
            _apiKey = Environment.GetEnvironmentVariable("SITE_API_KEY");
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
                var result = await _query.BuyJoyRub(JoyAmount);
                HttpClient _httpClient = new HttpClient();
                var request = new TelegramPaymentRequest
                {
                    user_id = result.Item2.TgUserId,
                    order_id = result.Item2.CodeOrder,
                    price = result.Item2.AmountPayment,
                    link = result.Item1.link_page_url,
                };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string _botApiUrl = "http://bot:5000/api/send";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
                try
                {
                    var response = await _httpClient.PostAsync(_botApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Успешно отправлено
                        _logger.LogInformation("Success Telegram Api");
                    }
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError(
                            $"Telegram API error: {response.StatusCode} - {errorBody}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Логируйте исключение
                    _logger.LogError($"Exception calling Telegram bot API: {ex.Message}");
                }
                return Ok("Оплата прошла успешно");
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
