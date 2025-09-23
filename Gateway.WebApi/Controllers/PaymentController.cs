using System.Security.Cryptography;
using System.Text;
using Business.Data.Enums;
using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.OrderQuery;
using Services.Payment.Dto;

namespace Gateway.WebApi.Controllers
{
    [Route("payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly OrderQuery _query;
        private readonly IRepository<Order> _orderRepository;
        private readonly string _apiKey;
        ILogger<OrderController> _logger;

        public PaymentController(
            OrderQuery query,
            IRepository<Order> orderRepository,
            ILogger<OrderController> logger,
            IConfiguration configuration
        )
        {
            _query = query;
            _orderRepository = orderRepository;
            _logger = logger;
            _apiKey = Environment.GetEnvironmentVariable("PAYMENT_API_KEY");
        }

        /// <summary>
        /// Обработка Postback уведомлений (Result URL)
        /// Этот URL должен быть указан в настройках магазина как Result URL
        /// </summary>
        [HttpPost("result")]
        public async Task<IActionResult> HandlePostback()
        {
            try
            {
                // Получаем данные из формы
                var form = await Request.ReadFormAsync();

                var model = new PaymentResultModel
                {
                    Status = form["Status"],
                    InvId = form["InvId"],
                    OutSum = form["OutSum"],
                    CurrencyIn = form["CurrencyIn"],
                    Commission = form["Commission"],
                    TrsId = form["TrsId"],
                    Custom = form["custom"],
                    SignatureValue = form["SignatureValue"],
                };

                _logger.LogInformation($"Received postback: {model.SignatureValue}");

                // Проверяем обязательные поля
                if (
                    string.IsNullOrEmpty(model.Status)
                    || string.IsNullOrEmpty(model.InvId)
                    || string.IsNullOrEmpty(model.OutSum)
                    || string.IsNullOrEmpty(model.SignatureValue)
                )
                {
                    _logger.LogWarning("Missing required fields in postback");
                    return BadRequest("Missing required fields");
                }

                // Находим заказ
                var order = (await _orderRepository.GetListQuery()).FirstOrDefault(o =>
                    o.OrderCode == model.InvId
                );

                if (order == null)
                {
                    _logger.LogWarning("Order not found for InvId: {InvId}", model.InvId);
                    return BadRequest("Order not found");
                }

                // Проверяем, не был ли уже обработан этот платеж
                if (
                    (order.Status == OrderStatus.Paid && model.Status == "SUCCESS")
                    || (order.Status == OrderStatus.NotPaid && model.Status == "FAIL")
                )
                {
                    _logger.LogInformation(
                        "Postback for order {InvId} already processed",
                        model.InvId
                    );
                    return Ok(); // Возвращаем 200 OK, чтобы платежная система не повторяла уведомление
                }

                // Обновляем статус
                if (model.Status == "SUCCESS")
                {
                    order.Status = OrderStatus.Paid;

                    await _orderRepository.Update(order);

                    _logger.LogInformation("Order {InvId} marked as paid", model.InvId);
                    return Ok();
                }
                else if (model.Status == "FAIL")
                {
                    order.Status = OrderStatus.NotPaid;

                    await _orderRepository.Update(order);

                    _logger.LogInformation("Order {InvId} marked as failed", model.InvId);
                    return Ok();
                }

                _logger.LogWarning(
                    "Unknown status received for order {InvId}: {Status}",
                    model.InvId,
                    model.Status
                );
                return BadRequest("Unknown status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing postback");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool VerifySignature(PaymentResultModel model)
        {
            string expectedSignature = GetSignature(model.OutSum, model.InvId, _apiKey);
            _logger.LogInformation($"Сигнатура: {expectedSignature}");
            return string.Equals(
                model.SignatureValue,
                expectedSignature,
                StringComparison.OrdinalIgnoreCase
            );
        }

        private string GetSignature(string outSum, string invId, string apiKey)
        {
            string signature = $"{outSum}:{invId}:{apiKey}";
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(signature));
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        /// <summary>
        /// Обработка успешного возврата пользователя (Success URL)
        /// Этот URL должен быть указан в настройках магазина как Success URL
        /// </summary>
        [HttpPost("success")]
        public IActionResult HandleSuccess()
        {
            var form = Request.ReadFormAsync().Result;

            var model = new
            {
                InvId = form["InvId"],
                OutSum = form["OutSum"],
                CurrencyIn = form["CurrencyIn"],
                Custom = form["custom"],
                SignatureValue = form["SignatureValue"],
            };

            // Проверяем подпись
            if (!VerifySignature(model.OutSum, model.InvId, _apiKey, model.SignatureValue))
            {
                _logger.LogWarning("Invalid signature for success redirect");
                return BadRequest("Invalid signature");
            }

            // Просто показываем страницу успеха, НЕ МЕНЯЕМ СТАТУС ЗАКАЗА
            return Ok(
                new
                {
                    success = true,
                    message = $"Ваш заказ {model.InvId} успешно оплачен. Благодарим за платёж!",
                    orderId = model.InvId,
                    amount = model.OutSum,
                    currency = model.CurrencyIn,
                }
            );
        }

        /// <summary>
        /// Обработка неуспешного возврата пользователя (Fail URL)
        /// Этот URL должен быть указан в настройках магазина как Fail URL
        /// </summary>
        [HttpPost("fail")]
        public IActionResult HandleFail()
        {
            var form = Request.ReadFormAsync().Result;

            var model = new
            {
                InvId = form["InvId"],
                OutSum = form["OutSum"],
                CurrencyIn = form["CurrencyIn"],
                Custom = form["custom"],
                SignatureValue = form["SignatureValue"],
            };

            // Проверяем подпись
            if (!VerifySignature(model.OutSum, model.InvId, _apiKey, model.SignatureValue))
            {
                _logger.LogWarning("Invalid signature for fail redirect");
                return BadRequest("Invalid signature");
            }

            // Просто показываем информацию об ошибке
            return Ok(
                new
                {
                    success = false,
                    message = "Оплата по заказу не прошла. Пожалуйста, попробуйте ещё раз или обратитесь в службу поддержки",
                    orderId = model.InvId,
                    amount = model.OutSum,
                    currency = model.CurrencyIn,
                    details = "Убедиться, что отправителю доступны операции в интернете, баланс карты достаточен для перевода, и повторить попытку. Средства холдируются на карте отправителя и списываются только в случае успешной авторизации на зачисление средств.",
                }
            );
        }

        private bool VerifySignature(
            string outSum,
            string invId,
            string apiKey,
            string signatureValue
        )
        {
            string expectedSignature = GetSignature(outSum, invId, apiKey);
            return string.Equals(
                signatureValue,
                expectedSignature,
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}
