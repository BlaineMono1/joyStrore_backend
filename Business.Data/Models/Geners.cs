using Business.Data.BaseEntities;


namespace Business.Data.Models
{
    public class Geners : BaseEntity
    {
        #region поля
        /// <summary>
        /// Жанр игры 
        /// </summary>
        public string? Name { get; set; }
        #endregion
        public List<GenersToEdition> Editions { get; set; }
        #region связи
        #endregion
    }
}
