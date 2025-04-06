using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    public class Role:BaseEntity
    {
        #region поля
        /// <summary>
        /// Наименование поля 
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region связи
        /// <summary>
        /// Пользователи 
        /// </summary>
        public List<Admin> Admins { get; set; }
        #endregion
    }
}
