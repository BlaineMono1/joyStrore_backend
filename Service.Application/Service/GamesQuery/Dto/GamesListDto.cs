
namespace Service.Application.Service.GamesQuery.Dto
{
    public class GamesListDto
    {
        public string FIlterName { get; set; }
        public Guid Id { get; set; }
        public string ImageFilepath { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? Jprice { get; set; }
        public string Discount { get; set; }
    }
}
