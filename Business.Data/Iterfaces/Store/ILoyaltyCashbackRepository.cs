using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface ILoyaltyCashbackRepository<T> : IRepository<LoyaltyCashback> where T : class, IBaseEntity
    {
        new Task Update(LoyaltyCashback entity);
    }
}
