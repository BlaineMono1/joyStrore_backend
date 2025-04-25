using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    public class Edition:BaseEntity
    {
        #region поля 
        /// <summary>
        /// Куса код игры Украина
        /// </summary>
        public string CusaCodeUa { get; set; }

        /// <summary>
        /// Куса код игры Турция
        /// </summary>
        public string CusaCodeTr { get; set; }

        /// <summary>
        /// Тип объекта(Игра)
        /// </summary>
        public string?Type {  get; set; }

        /// <summary>
        /// Наименование издания 
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Тип издания 
        /// </summary>
        public string EditionType { get; set; }
        /// <summary>
        /// Url изображения
        /// </summary>
        public string? Image {  get; set; }
        /// <summary>
        /// Платформа игры PS4 | PS5
        /// </summary>
        public string? Platform { get; set; }
        /// <summary>
        /// Входит в подписку 
        /// </summary>
        public string? Subscription {  get; set; }
        /// <summary>
        /// Что входит в издание
        /// </summary>
        public string? Features { get; set; }

        /// <summary>
        /// Релиз игры
        /// </summary>
        public DateTime? Release { get; set; }

        /// <summary>
        /// Регион в котором доступна игра
        /// </summary>
        public string Region { get; set; }
        #endregion

        #region связи
        /// <summary>
        /// Id игры концепт
        /// </summary>
        public Guid GameId { get; set; }
        [ForeignKey("GameId")]
        public Game Game {  get; set; }
        /// <summary>
        /// Id товара 
        /// </summary>
        public Guid  ProductId {  get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        /// <summary>
        /// Жанры игры
        /// </summary>
        public List<GenersToEdition> EditionGeners { get; set; }

        #endregion

    }
}
