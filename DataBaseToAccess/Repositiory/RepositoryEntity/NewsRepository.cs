using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class NewsRepository<T> : Repository<News>, INewsRepository<T> where T : class, IBaseEntity
    {
        public NewsRepository(BaseDbContext contex) : base(contex) { }
    }
}
