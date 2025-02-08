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

        #region связи
        public List<Edition>? Editions { get; set; }
        #endregion
    }
}
