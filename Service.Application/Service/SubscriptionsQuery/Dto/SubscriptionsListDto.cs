namespace Service.Application.Service.SubscriptionsQuery.Dto
{
    public class SubscriptionsListDto
    {
        public Guid id { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public decimal? JpriceUa { get; set; }
        public decimal? JpriceTr { get; set; }
        public decimal? PriceUa { get; set; }
        public decimal? PriceTr { get; set; }
    }
}
