using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    /// <summary>
    /// История покупок пользователя 
    /// </summary>
    public class ProductTransactionHistory:BaseEntity
    {
        #region поля 
        #endregion


        #region связи
        /// <summary>
        /// Пользователь
        /// </summary>
       
        public User User { get; set; }


        /// <summary>
        /// Ордера пользователя 
        /// </summary>
        public List<Order> Orders { get; set; }
        #endregion
    }
}
