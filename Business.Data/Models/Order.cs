using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;
using Business.Data.Enums;

namespace Business.Data.Models
{
    /// <summary>
    /// Ордер
    /// </summary>
    public class Order : BaseEntity
    {
        #region поля
        /// <summary>
        /// UserID tg
        /// </summary>
        public string TgUserId { get; set; }

        /// <summary>
        /// Логин аккаунта ps
        /// </summary>
        public string PsLogin { get; set; }

        /// <summary>
        /// Пароль аккаунта ps
        /// </summary>
        public string PsPass { get; set; }

        /// <summary>
        /// Одноразовый код на вход
        /// </summary>
        public string Code { get; set; }

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
        /// Оплечен ли тавар коинами
        /// </summary>
        public bool IsJPayment { get; set; }

        /// <summary>
        /// Guid воркера\админа кто взял заказ
        /// </summary>
        ///
        public Guid? WorkerId { get; set; }

        /// <summary>
        /// Регион заказа
        /// </summary>
        ///
        public string Region { get; set; }

        /// <summary>
        /// Количество Joy+ за заказ
        /// </summary>
        ///
        public decimal TotalJoyPlus { get; set; }

        public string NewAccount { get; set; }

        #endregion


        #region связи
        /// <summary>
        /// Item истории транзакции пользователя
        /// </summary>
        //public Guid ProductTransactionItemId {  get; set; }
        //[ForeignKey("ProductTransactionItemId")]
        //public ProductTransactionItem ProductTransactionItem { get; set; }
        /// <summary>
        /// Товар
        /// </summary>
        public List<OrderProductItem> OrderProductItems { get; set; }
        #endregion
    }
}
