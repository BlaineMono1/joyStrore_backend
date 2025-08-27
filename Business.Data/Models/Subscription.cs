using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    public class Subscription : BaseEntity
    {
        #region поля
        /// <summary>
        /// Куса код подписки Украина
        /// </summary>
        public string CusaCodeUa { get; set; }

        /// <summary>
        /// Куса код подписки Турция
        /// </summary>
        public string CusaCodeTr { get; set; }

        /// <summary>
        /// Наименование Подписки
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Тип объекта(Subscription)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Url изображения
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Url изображения для главной страницы
        /// </summary>
        public string? ImageLayout { get; set; }

        /// <summary>
        /// Платформа игры PS4 | PS5
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// Длительность
        /// </summary>
        public string? Duration { get; set; }

        /// <summary>
        /// Имя раздела
        /// </summary>
        public string? SectionName { get; set; }

        /// <summary>
        /// Имя раздела
        /// </summary>
        public string? Variety { get; set; }

        #endregion

        #region связи
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public List<PriceSettingSubscription>? PriceSettingSubscriptions { get; set; }
        #endregion
    }
}
