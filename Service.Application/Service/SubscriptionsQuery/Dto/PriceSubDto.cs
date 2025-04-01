namespace Service.Application.Service.SubscriptionsQuery.Dto
{
    public class PriceSubDto
    {
        public Guid Id { get; set; }
        public decimal? PriceUAH { get; set; }
        public decimal? PriceTRY { get; set; }
        public string SectionName { get; set; }
        public string Duration { get; set; }
    }
}
