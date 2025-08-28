namespace Service.Application.Response;

public class CreatePaymentResponse
{
    public bool success { get; set; }
    public string bill_id { get; set; }
    public string link_url { get; set; }
    public string link_page_url { get; set; }
}
