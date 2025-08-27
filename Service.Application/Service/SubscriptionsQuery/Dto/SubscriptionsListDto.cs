namespace Service.Application.Service.SubscriptionsQuery.Dto
{
    public class SubscriptionsLayout
    {
        public string Name { get; set; }
        public List<SubscriptionsListDto> subscriptionsListDtos = new List<SubscriptionsListDto>();
    }

    public class SubscriptionsListDto
    {
        public Guid ProductId { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public decimal? Jprice { get; set; }
        public decimal? Price { get; set; }
        public string Dicount { get; set; }
        public string SectionName { get; set; }
    }
}
