using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    public class User:BaseEntity
    {
        #region поля 
        /// <summary>
        /// UserID tg
        /// </summary>
        public string TgUserId { get; set; }

        /// <summary>
        /// Платформа игры Ps4 или Ps5
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Почта пользователя для отпарвки чека 
        /// </summary>
        public string? Email { get; set; }
        #endregion

        #region связи
       
        /// <summary>
        /// Настройки пользователя
        /// </summary>
        public List<Setting> Settings { get; set; }
        /// <summary>
        /// Корзина
        /// </summary>
        public Guid CartId { get; set; }
        [ForeignKey("CartId")]
        public Cart Cart { get; set; }
        /// <summary>
        /// Избранное 
        /// </summary>
        public Guid FavoriteId {  get; set; }
        [ForeignKey("FavoriteId")]
        public Favorite Favorite { get; set; }

        /// <summary>
        /// Баланс Joy и JoyPlus пользователя 
        /// </summary>
        public Guid LoyaltyCurrencyId {  get; set; }
        [ForeignKey("LoyaltyCurrencyId")]
        public LoyaltyCurrency LoyaltyCurrency { get; set; }

        /// <summary>
        /// История покупок пользователя 
        /// </summary>
        public Guid ProductTransactionHistoryId { get; set; }
        [ForeignKey("ProductTransactionHistoryId")]
        public ProductTransactionHistory ProductTransactionHistory { get; set; }

        #endregion

    }
}
