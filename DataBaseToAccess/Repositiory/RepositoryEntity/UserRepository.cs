using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class UserRepository<T> : Repository<User>, IUserRepository<T> where T : class, IBaseEntity
    {
        public UserRepository(BaseDbContext contex) : base(contex) { }

        public async Task<User> GetUserByTgId(string tgId)
        {
            return (await GetListQuery())
                .FirstOrDefault(u => u.TgUserId == tgId);

        }
    }
}
