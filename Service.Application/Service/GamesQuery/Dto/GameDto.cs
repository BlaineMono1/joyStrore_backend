using Business.Data.Models;

namespace Service.Application.Service.GamesQuery.Dto
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public string Image { get; set; }
        public List<Geners> Geners { get; set; }
        public DateTime RealiseDate { get; set; }
        public string Platforms { get; set; }
        public string Languages { get; set; }
        public List<Edition> Editions { get; set; }
        public string Subscription { get; set; }
        public DateTime? Discount { get; set; }
        public string Features { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public decimal JPlus { get; set; }
        public string DiscountPercent { get; set; }
        public bool InCart { get; set; }
        public bool InFavorite { get; set; }
    }
}
