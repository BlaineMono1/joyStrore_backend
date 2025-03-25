using Business.Data.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Data.Models
{
    public class SectionsEditions : BaseEntity
    {
        [ForeignKey("EdtitonId")]
        public Guid EdtitonId { get; set; }
        public Edition Edition { get; set; }

        [ForeignKey("SectionId")]
        public Guid SectionId { get; set; }
        public Section Section { get; set; }
    }
}
