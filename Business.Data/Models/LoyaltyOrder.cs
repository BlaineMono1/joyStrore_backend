using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;
using Business.Data.Enums;

namespace Business.Data.Models
{
    /// <summary>
    /// Ордер на покупку Joy
    /// </summary>
    public class LoyaltyOrder : BaseEntity
    {
        #region поля
        /// <summary>
        /// UserID tg
        /// </summary>
        public string TgUserId { get; set; }

        /// <summary>
        /// Кол-во покупаемых Joy
        /// </summary>
        public decimal CountProductJoy { get; set; }

        /// <summary>
        /// Кол-во оплаты
        /// </summary>
        public decimal AmountPayment { get; set; }

        /// <summary>
        /// Номер заказа
        /// </summary>
        public string CodeOrder { get; set; }

        /// <summary>
        /// Чем оплачено
        /// </summary>
        public bool ByJoyPlus { get; set; }

        public OrderStatus Status { get; set; }

        #endregion
    }
}
