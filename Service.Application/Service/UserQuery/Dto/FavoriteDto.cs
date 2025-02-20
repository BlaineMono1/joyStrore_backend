
namespace Service.Application.Service.UserQuery.Dto
{
    public class FavoriteDto
    {
        public Guid Id { get; set; }
        public string image { get; set; }
        public string Name { get; set; }
        public string EditionName { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public DateTime? DiscountTime { get; set; }
        public string Discount { get; set; }
        public bool InCart { get; set; }
    }
}
