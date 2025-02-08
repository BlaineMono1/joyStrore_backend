using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class EditionRepository<T> : Repository<Edition>, IEditionRepository<T> where T : class, IBaseEntity
    {
        public EditionRepository(BaseDbContext contex) : base(contex) { }

        public async Task<List<Edition>> GetEditions(Guid GameID)
        {
            var Editions = (await GetAllList()).Where(e => e.GameId == GameID).ToList();

            return Editions;
        }
    }
}
