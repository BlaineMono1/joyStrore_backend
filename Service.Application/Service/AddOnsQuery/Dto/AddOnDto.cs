
using Business.Data.Models;

namespace Service.Application.Service.AddOnsQuery.Dto
{
    public class AddOnDto
    {
        public Guid Id { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Platform { get; set; }
        public List<AddOn> AddOns { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public decimal JPlus { get; set; }
    }
}
