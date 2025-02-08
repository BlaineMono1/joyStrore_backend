using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class GenersRepository<T> : Repository<Geners>, IGenersRepository<T> where T : class, IBaseEntity
    {
        public GenersRepository(BaseDbContext contex) : base(contex) { }

        public async Task<List<string>> GetGeners(Guid EditionID)
        {
            var result = (await GetAllList())
                .Where(g => g.Editions.Any(e => e.Guid == EditionID))
                    .Select(g => g.Name)
                    .ToList();

            return result
                ?? throw new Exception("Geners is null");

        }
    }
}
