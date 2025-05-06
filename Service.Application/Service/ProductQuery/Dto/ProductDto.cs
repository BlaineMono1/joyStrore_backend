using Service.Application.Service.AddOnsQuery.Dto;
namespace Service.Application.Service.ProductQuery.Dto
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string ProductType { get; set; }
        public string Image { get; set; }
        public List<string>? Geners { get; set; }
        public DateTime? RealiseDate { get; set; }
        public string Platforms { get; set; }
        public string? Languages { get; set; }
        //public List<DropDownListDto> DropDownList { get; set; }
        public string? Subscription { get; set; }
        public DateTime? Discount { get; set; }
        public string? Features { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public decimal JPlus { get; set; }
        public string DiscountPercent { get; set; }
        public bool InCart { get; set; }
        public bool InFavorite { get; set; }
        //public List<GameAddOnListDto>? Addons { get; set; }
        public bool IsPlatform { get; set; }
    }
}
