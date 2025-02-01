using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class SubscriptionRepository<T> : Repository<Subscription>, ISubscriptionRepository<T> where T : class, IBaseEntity
    {
        public SubscriptionRepository(BaseDbContext contex) : base(contex) { }
    }
}
