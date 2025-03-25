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
            return await _context.Set<T>().AsNoTracking().Where(e => !e.IsDelete).ToListAsync();
        }

        /// <summary>
        /// Получить объект по id 
        /// </summary>
        /// <returns></returns>
        public async Task<T?> GetById(Guid id)
        {
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(x => x.Guid == id);
        }

        /// <summary>
        /// Обновить объект 
        /// </summary>
        /// <returns></returns>
        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }



        /// <summary>
        /// Мягко удалить объект 
        /// </summary>
        /// <returns></returns>
        public async Task SoftDelete(Guid id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                entity.IsDelete = true;
                await Update(entity);
            }

        }


        /// <summary>
        /// Жестко удалить объект 
        /// </summary>
        /// <returns></returns>
        public async Task HardDelete(Guid id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _context.Remove(entity);
                await _context.SaveChangesAsync();
            }

        }

        /// <summary>
        /// Добавить объект 
        /// </summary>
        /// <returns></returns>
        public async Task Add(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IQueryable<T>> GetListQuery()
        {
            return _context.Set<T>().AsNoTracking().Where(_ => !_.IsDelete).AsQueryable();
        }

        public async Task SaveDb()
        {
            await _context.SaveChangesAsync();
        }
    }
}
