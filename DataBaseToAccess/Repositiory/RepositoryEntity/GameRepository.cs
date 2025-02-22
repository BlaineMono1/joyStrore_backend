
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class GameRepository<T> : Repository<Game>, IGameRepository<T> where T : class, IBaseEntity
    {
        public GameRepository(BaseDbContext contex) : base(contex) { }

        public async Task<List<AddOn>> AddOnsByGame(Guid AddOnId)
        {
            var games = await GetAllList();
            var addOns = (await GetAllList())
                .Where(game => game.AddOns.Any(a => a.Guid == AddOnId))
                    .SelectMany(game => game.AddOns)
                        .ToList();

            return addOns;
        }
        public async Task<List<Game>> FilterGames(string name, List<string> Geners)
        {
            var gamesByName = (await GetAllList()).Where(g => g.Name.Contains(name));

            var gamesFilter = gamesByName.Where(g => g.Editions.Any(e => e.Geners.All(g => Geners.Contains(g.Name))));

            return gamesFilter.ToList();
        }

    }
}
