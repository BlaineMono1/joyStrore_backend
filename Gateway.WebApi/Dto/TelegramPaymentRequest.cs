namespace Gateway.WebApi.Dto;

public class TelegramPaymentRequest
{
    public string user_Id { get; set; } // Telegram user_id — обычно long
    public string order_id { get; set; } // Номер заказа
    public decimal price { get; set; } // Сумма (строка, например "1500₽")
    public string link { get; set; } // Ссылка на оплату
}
