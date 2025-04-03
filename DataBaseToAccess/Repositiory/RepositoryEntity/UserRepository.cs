using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class UserRepository<T> : Repository<User>, IUserRepository<T> where T : class, IBaseEntity
    {
        private readonly BaseDbContext _contex;
        public UserRepository(BaseDbContext contex) : base(contex) 
        {
            _contex = contex;
        }

        public async Task<User> GetUserByTgId(string tgId)
        {
            return await _contex.Set<User>().AsNoTracking().FirstOrDefaultAsync(u => u.TgUserId == tgId);

        }

        public async Task<IQueryable<User>> GetDeletedQuery()
        {
            return _contex.Set<User>().AsNoTracking().Where(u => u.IsDelete).AsQueryable();
        }
    }
}
