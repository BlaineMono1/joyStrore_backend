using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    /// <summary>
    /// Данные админа 
    /// </summary>
    public class Admin:BaseEntity
    {
        #region поля
        public string Login { get; set; }
        public string Password { get; set; }

        #endregion

        #region связи 
        /// <summary>
        /// Права 
        /// </summary>
        public Guid RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
        #endregion

    }
}
