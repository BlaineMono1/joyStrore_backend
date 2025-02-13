
namespace Service.Application.Service.UserQuery.Dto
{
    public class CartItemDto
    {
        public Guid GameId { get; set; }
        public string image { get; set; }
        public string Name { get; set; }
        public string EditionName { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public string Discount { get; set; }
        public string Platform { get; set; }
    }
}
