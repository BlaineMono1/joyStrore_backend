using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    public class Product:BaseEntity
    {
        #region поля
        /// <summary>
        /// Id Издания/AddOn/Подписки
        /// </summary>
        public Guid TypeId { get; set; }
        /// <summary>
        /// Тип объекта Издания/AddOn/Подписки
        /// </summary>
        public string? Type { get; set; }
        /// <summary>
        /// Цена в гривнах
        /// </summary>
        public decimal? PriceUa { get; set; }
        /// <summary>
        /// Цена в лирах
        /// </summary>
        public decimal? PriceTr {  get; set; }
        /// Процент скидки UAH
        /// </summary>
        public string? DiscountPercentUa {  get; set; }
        /// Процент скидки TRY
        /// </summary>
        public string? DiscountPercentTr { get; set; }
        /// <summary>
        /// Длительность скидки UAH
        /// </summary>
        public DateTime? DiscountDateUa { get; set; }
        /// <summary>
        /// Длительность скидки TRY
        /// </summary>
        public DateTime? DiscountDateTr { get; set; }


        #endregion

        #region связи
        public AddOn? AddOn { get; set; }
        public Subscription? Subscription { get; set; }
        public Edition? Edition { get; set; }

        public List<SectionsProducts> Sections { get; set; }
        #endregion

    }
}
