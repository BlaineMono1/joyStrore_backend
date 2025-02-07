using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    /// <summary>
    /// Игры концепция 
    /// </summary>
    public class Game:BaseEntity
    {
        #region поля
        /// <summary>
        /// Id игры с PsStore
        /// </summary>
        public string ConceptId { get; set; }
        /// <summary>
        /// Наименование игры
        /// </summary>
        public string? Name { get; set; }
               
        /// <summary>
        /// Язык игры
        /// </summary>
        public string? Languages {  get; set; }
        /// <summary>
        /// Релиз игры
        /// </summary>
        public DateTime? Release {  get; set; }
        /// <summary>
        /// В каком регионе игра есть (UA | TR)
        /// </summary>
        public string? Region {  get; set; }
        #endregion

        #region связи
        /// <summary>
        /// Издания
        /// </summary>
        public List<Edition> Editions { get; set; }
        /// <summary>
        /// AddOns
        /// </summary>
        public List<AddOn> AddOns { get; set; }

        /// <summary>
        /// Жанры игры
        /// </summary>
        public List<Geners> Geners { get; set; }
        #endregion


    }
}
