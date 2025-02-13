
namespace Service.Application.Service.UserQuery.Dto
{
    public class FavoriteDto
    {
        public Guid GameId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Edition { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public DateTime? DiscountTime { get; set; }
        public string Discount { get; set; }
        public bool InCart { get; set; }
    }
}
