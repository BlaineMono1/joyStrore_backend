namespace Gateway.WebApi.Dto;

public class TelegramPaymentRequest
{
    public string UserId { get; set; } // Telegram user_id — обычно long
    public string OrderId { get; set; } // Номер заказа
    public decimal Price { get; set; } // Сумма (строка, например "1500₽")
    public string Link { get; set; } // Ссылка на оплату
}
