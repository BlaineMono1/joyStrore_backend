using System;
using System.Text.Json.Serialization;

namespace Services.Payment.Dto;

public class PaymentResultModel
{
    public string Status { get; set; }
    public string InvId { get; set; }
    public string OutSum { get; set; }
    public string CurrencyIn { get; set; }
    public string Commission { get; set; } // Добавьте это поле
    public string TrsId { get; set; }

    [JsonPropertyName("custom")] // Используйте атрибут для корректной десериализации
    public string Custom { get; set; }
    public string SignatureValue { get; set; }
}
