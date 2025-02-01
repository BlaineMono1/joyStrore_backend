using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class GameRepository<T> : Repository<Game>, IGameRepository<T> where T : class, IBaseEntity
    {
        public GameRepository(BaseDbContext contex) : base(contex) { }
    }
    
}
