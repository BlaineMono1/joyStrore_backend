namespace Services.Payment.Dto;

public class PaymentParametersDto
{
    public decimal? Ammount { get; set; }
    public string? Order_Id { get; set; }
    public string Type { get; set; }

    //Валюта, в которой оплачивается счет
    public string Currency_in { get; set; }
    public string? Name { get; set; }
    public int Ttl { get; set; }
}
