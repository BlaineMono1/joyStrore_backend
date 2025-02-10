
using Business.Data.Models;

namespace Service.Application.Service.SubscriptionsQuery.Dto
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Platform { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public string Discount { get; set; }
        public List<Subscription> Subscriptions { get; set; }

    }
}
