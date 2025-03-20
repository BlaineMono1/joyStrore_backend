
namespace Service.Application.Service.AddOnsQuery.Dto
{
    public class GroupAddOnsDto
    {
        public Guid ProductId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public string Discount { get; set; }

    }
}
