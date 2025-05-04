
namespace Service.Application.Service.OrderQuery.Dto
{
    public class UserOrderItems
    {
        public Guid ProdcuctId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string EditionType { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public string Percent { get; set; }
        public string Platform { get; set; }
    }
}
