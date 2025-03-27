namespace Service.Application.Service.SectionQuery.Dto
{
    public class SectionDto
    {
        public Guid SectionId { get; set; }
        public string SectionName { get; set; }
        public new List<ProductDto> Products { get; set; }
        
    }
}
