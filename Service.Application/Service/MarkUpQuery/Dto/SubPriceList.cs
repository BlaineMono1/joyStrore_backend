namespace Service.Application.Service.MarkUpQuery.Dto;

public class SubPriceList
{
    public string Duration { get; set; }
    public List<SubscriptionItem>? Items { get; set; }
}

public class SubscriptionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal PriceUa { get; set; }
    public decimal PriceTr { get; set; }
}
