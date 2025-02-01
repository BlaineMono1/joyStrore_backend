using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IAddOnRepository<T> : IRepository<AddOn> where T : class, IBaseEntity
    {
    }
}
