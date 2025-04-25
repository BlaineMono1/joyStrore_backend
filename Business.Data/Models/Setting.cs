using System.ComponentModel.DataAnnotations.Schema;
using Business.Data.BaseEntities;

namespace Business.Data.Models
{
    public class Setting:BaseEntity
    {
        #region поля
        /// <summary>
        /// Настройка для определенного региона UA или TR
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// Почта от аккаунта PsStore
        /// </summary>
        public string? EmailPsStore { get; set; }
        /// <summary>
        /// Пароль от PsStore
        /// </summary>
        public string? PasswordPsStore { get; set; }
        /// <summary>
        /// Одноразовый код
        /// </summary>
        public string? Code { get; set; }

        #endregion

        #region связи
        /// <summary>
        /// Пользователь
        /// </summary>
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        #endregion
    }
}
