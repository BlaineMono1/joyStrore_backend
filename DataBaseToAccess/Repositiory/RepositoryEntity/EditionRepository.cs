using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class EditionRepository<T> : Repository<Edition>, IEditionRepository<T> where T : class, IBaseEntity
    {
        public EditionRepository(BaseDbContext contex) : base(contex) { }

        public async Task<List<Edition>> GetEditions(Guid GameID)
        {
            var Editions = (await GetListQuery()).Where(e => e.GameId == GameID).Include(e => e.EditionGeners).ThenInclude(e => e.Geners);

            return Editions.ToList();
        }
    }
}
