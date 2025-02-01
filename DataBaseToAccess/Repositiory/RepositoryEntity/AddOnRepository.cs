using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class AddOnRepository<T> : Repository<AddOn>, IAddOnRepository<T> where T : class, IBaseEntity
    {
        public AddOnRepository(BaseDbContext contex) : base(contex) { }

    }
}
