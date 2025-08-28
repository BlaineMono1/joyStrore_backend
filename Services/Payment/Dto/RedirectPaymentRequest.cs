namespace Services.Payment.Dto;

public class RedirectPaymentRequest
{
    public string InvId { get; set; }
    public string OutSum { get; set; }
    public string CurrencyIn { get; set; }
    public string SignatureValue { get; set; }
}
