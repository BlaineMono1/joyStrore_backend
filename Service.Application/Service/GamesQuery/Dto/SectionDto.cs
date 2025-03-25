namespace Service.Application.Service.GamesQuery.Dto
{
    public class SectionDto
    {
        public string Name { get; set; }

        public List<GamesListDto> Editions { get; set; } = new List<GamesListDto>();

    }
}
