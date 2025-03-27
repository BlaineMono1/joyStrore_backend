using Business.Data.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Data.Models
{
    public class SectionsProducts : BaseEntity
    {
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; } 
        public Product Product { get; set; }

        [ForeignKey("SectionId")]
        public Guid SectionId { get; set; }
        public Section Section { get; set; }
    }
}
