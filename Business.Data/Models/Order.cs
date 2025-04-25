using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;
using Business.Data.Enums;

namespace Business.Data.Models
{
    /// <summary>
    /// Ордер
    /// </summary>
    public class Order:BaseEntity
    {
        #region поля 
        /// <summary>
        /// UserID tg
        /// </summary>
        public string TgUserId { get; set; }
        /// <summary>
        /// Номер заказа
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Сумма за заказ в рублях
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Сумма за заказ в коинах
        /// </summary>
        public decimal JPrice { get; set; }

        #endregion


        #region связи
        /// <summary>
        /// Item истории транзакции пользователя 
        /// </summary>
        public Guid ProductTransactionItemId {  get; set; }
        [ForeignKey("ProductTransactionItemId")]
        public ProductTransactionItem ProductTransactionItem { get; set; }
        /// <summary>
        /// Товар
        /// </summary>
        public List<OrderProductItem> OrderProductItems { get; set; }
        #endregion
    }
}
