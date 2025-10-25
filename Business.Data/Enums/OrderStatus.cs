namespace Business.Data.Enums
{
    public enum OrderStatus
    {
        /// <summary>
        /// Создан но не прошел проверку на оплату
        /// </summary>
        Created,

        //В процессе
        Processing,

        //Завершен
        Completed,

        //отменен
        Cancelled,

        //Оплачен
        Paid,

        //false при проверку на оплату. Не оплачен
        NotPaid,
    }
}
