namespace Service.Application.Service.GamesQuery.Dto
{
    public class AddOnDto
    {
        public Guid Id { get; set; }
        public string AddOnName { get; set; }
        public string GameName { get; set; }
        public string Image { get; set; }
        public string Platform { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public string DiscountPercent { get; set; }

    }
}
