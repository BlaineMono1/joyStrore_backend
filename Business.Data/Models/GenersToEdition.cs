using Business.Data.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Data.Models
{
    public class GenersToEdition : BaseEntity
    {
        
        [ForeignKey("EdtitonId")]
        public Guid EdtitonId { get; set; }
        public Edition Edition { get; set; }
        [ForeignKey("GenerId")]
        public Guid GenerId { get; set; }
        public Geners Geners { get; set; }

    }
}
