using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface ISubscriptionRepository<T>:IRepository<Subscription> where T : class, IBaseEntity
    {
        /// <summary>
        /// Получить список подписок по имени
        /// </summary>
        /// <returns></returns>
        Task<List<Subscription>> SubscriptionsByName(string name);
    }
}
