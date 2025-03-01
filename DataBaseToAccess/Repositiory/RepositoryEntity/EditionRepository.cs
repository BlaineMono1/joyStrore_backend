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

        public async Task<List<Edition>> FilterEditions(string? name, List<string>? FilterGeners)
        {
            var gamesByName = await GetListQuery();

            if (!string.IsNullOrEmpty(name))
            {
                gamesByName = (await GetListQuery()).Where(e => e.EditionName.ToLower().Contains(name.ToLower())).Include(e => e.EditionGeners).ThenInclude(e => e.Geners);
            }

            var gamesFilter = gamesByName;
            if(FilterGeners != null && FilterGeners.Any())
            gamesFilter = gamesByName.Where(e => e.EditionGeners.Any(g => FilterGeners.Contains(g.Geners.Name)));

            return gamesFilter.ToList();
        }
    }
}
