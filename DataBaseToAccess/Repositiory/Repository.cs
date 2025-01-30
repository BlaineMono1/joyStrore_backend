using Business.Data.Iterfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseToAccess.Repositiory
{
    public class Repository<T> : IRepository<T> where T : class, IBaseEntity
    {
        private readonly BaseDbContext _context;
        public Repository(BaseDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Получить список объектов 
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetAllList()
        {
             return await _context.Set<T>().AsNoTracking().Where(p => !p.IsDelete).ToListAsync();
        }
    }
}
