using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface ISubscriptionRepository<T> : IRepository<Subscription> where T : class, IBaseEntity
    {
    }
}
