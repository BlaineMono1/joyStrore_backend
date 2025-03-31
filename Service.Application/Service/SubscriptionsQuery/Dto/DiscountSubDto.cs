namespace Service.Application.Service.SubscriptionsQuery.Dto
{
    public class DiscountSubDto
    {
        public Guid Id { get; set; }
        public string Percent { get; set; }
        public string SectionName { get; set; }
        public string Duration { get; set; }
    }
}
