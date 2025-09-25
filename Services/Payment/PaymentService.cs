using System.Globalization;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Application.Iterfaces;
using Service.Application.Response;
using Services.Payment.Dto;

namespace Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly string _apiKey;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("PAYMENT_API_KEY");
    }

    public async Task<CreatePaymentResponse> CreatePayment(Order order)
    {
        _logger.LogInformation("Созается ссылка на оплату");
        var urlPay = "https://pal24.pro/api/v1/bill/create";
        // Создаем DTO с параметрами оплаты на основе данных заказа
        var paymentParams = new PaymentParametersDto
        {
            Ammount = order.Price, // Используем сумму из заказа
            Order_Id = order.OrderCode, // Используем код заказа
            Type = "normal", // По умолчанию одноразовый, можно изменить при необходимости
            Currency_in = "RUB", // Валюта по умолчанию
            Name = $"Order #{order.OrderCode}", // Название, например, "Order #12345"
            Ttl = 600, // Время жизни счета по умолчанию (10 минут)
        };

        // Создаем словарь параметров для отправки
        var parameters = new Dictionary<string, string>
        {
            ["amount"] = order.Price.ToString("F2", CultureInfo.InvariantCulture),
            ["shop_id"] = "JDmGy4l7Q0",
            ["order_id"] = paymentParams.Order_Id,
            ["description"] = $"Order #{order.OrderCode}",
            ["type"] = paymentParams.Type,
            ["currency_in"] = paymentParams.Currency_in,
            ["name"] = paymentParams.Name,
            ["ttl"] = paymentParams.Ttl.ToString(),
        };
        // Преобразуем параметры в формат form-data
        var content = new FormUrlEncodedContent(parameters);

        try
        {
            HttpClient _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"{_apiKey}");
            var response = await _httpClient.PostAsync(urlPay, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(
                $"API: {response.StatusCode} - {response.ReasonPhrase}\nResponse body: {responseContent}"
            );
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new Exception("API Token is invalid or missing");
                case System.Net.HttpStatusCode.Forbidden:
                    // Анализируем содержимое ответа для получения конкретной ошибки
                    if (responseContent.Contains("api:error.invalid_amount"))
                        throw new Exception("Invalid amount provided");
                    else if (responseContent.Contains("api:error.merchant_banned"))
                        throw new Exception("Merchant access is banned");
                    else if (responseContent.Contains("api:error.merchant_not_found"))
                        throw new Exception("Merchant not found");
                    else if (responseContent.Contains("api:error.shop_not_found"))
                        throw new Exception("Shop not found");
                    else if (responseContent.Contains("api:error.shop_not_enabled"))
                        throw new Exception("Shop is not enabled");
                    else if (responseContent.Contains("api:error.access_denied"))
                        throw new Exception("Access denied to merchant");
                    else if (responseContent.Contains("api:error.rate-not-found"))
                        throw new Exception("Rate direction is not available");
                    else
                        throw new Exception("Forbidden access to payment system");
                case System.Net.HttpStatusCode.UnprocessableEntity:
                    throw new Exception("Validation error in input data");
                case System.Net.HttpStatusCode.InternalServerError:
                    throw new Exception("Internal server error in payment system");
                case System.Net.HttpStatusCode.OK:
                    return JsonConvert.DeserializeObject<CreatePaymentResponse>(responseContent);
                default:
                    throw new Exception($"HTTP Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (HttpRequestException hre)
        {
            throw new Exception($"Network error while creating payment: {hre.Message}", hre);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create payment: {ex.Message}", ex);
        }
    }

    public async Task<CreatePaymentResponse> CreatePayment(LoyaltyOrder order)
    {
        _logger.LogInformation("Созается ссылка на оплату");
        var urlPay = "https://pal24.pro/api/v1/bill/create";
        // Создаем DTO с параметрами оплаты на основе данных заказа
        var paymentParams = new PaymentParametersDto
        {
            Ammount = order.AmountPayment, // Используем сумму из заказа
            Order_Id = order.CodeOrder, // Используем код заказа
            Type = "normal", // По умолчанию одноразовый, можно изменить при необходимости
            Currency_in = "RUB", // Валюта по умолчанию
            Name = $"Order_Joy_Buy #{order.CodeOrder}", // Название, например, "Order #12345"
            Ttl = 600, // Время жизни счета по умолчанию (10 минут)
        };

        // Создаем словарь параметров для отправки
        var parameters = new Dictionary<string, string>
        {
            ["amount"] = order.AmountPayment.ToString("F2", CultureInfo.InvariantCulture),
            ["shop_id"] = "JDmGy4l7Q0",
            ["order_id"] = paymentParams.Order_Id,
            ["description"] = $"Order #{order.CodeOrder}",
            ["type"] = paymentParams.Type,
            ["currency_in"] = paymentParams.Currency_in,
            ["name"] = paymentParams.Name,
            ["ttl"] = paymentParams.Ttl.ToString(),
        };
        // Преобразуем параметры в формат form-data
        var content = new FormUrlEncodedContent(parameters);

        try
        {
            HttpClient _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"{_apiKey}");
            var response = await _httpClient.PostAsync(urlPay, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                $"API Error: {response.StatusCode} - {response.ReasonPhrase}\nResponse body: {responseContent}"
            );
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new Exception("API Token is invalid or missing");
                case System.Net.HttpStatusCode.Forbidden:
                    // Анализируем содержимое ответа для получения конкретной ошибки
                    if (responseContent.Contains("api:error.invalid_amount"))
                        throw new Exception("Invalid amount provided");
                    else if (responseContent.Contains("api:error.merchant_banned"))
                        throw new Exception("Merchant access is banned");
                    else if (responseContent.Contains("api:error.merchant_not_found"))
                        throw new Exception("Merchant not found");
                    else if (responseContent.Contains("api:error.shop_not_found"))
                        throw new Exception("Shop not found");
                    else if (responseContent.Contains("api:error.shop_not_enabled"))
                        throw new Exception("Shop is not enabled");
                    else if (responseContent.Contains("api:error.access_denied"))
                        throw new Exception("Access denied to merchant");
                    else if (responseContent.Contains("api:error.rate-not-found"))
                        throw new Exception("Rate direction is not available");
                    else
                        throw new Exception("Forbidden access to payment system");
                case System.Net.HttpStatusCode.UnprocessableEntity:
                    throw new Exception("Validation error in input data");
                case System.Net.HttpStatusCode.InternalServerError:
                    throw new Exception("Internal server error in payment system");
                case System.Net.HttpStatusCode.OK:
                    return JsonConvert.DeserializeObject<CreatePaymentResponse>(responseContent);
                default:
                    throw new Exception($"HTTP Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (HttpRequestException hre)
        {
            throw new Exception($"Network error while creating payment: {hre.Message}", hre);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create payment: {ex.Message}", ex);
        }
    }
}
