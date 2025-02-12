using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class UserRepository<T> : Repository<User>, IUserRepository<T> where T : class, IBaseEntity
    {
        public UserRepository(BaseDbContext contex) : base(contex) { }

        public async Task<User> GetUserByTgId(string tgId)
        {
            return (await GetAllList()).FirstOrDefault(u => u.TgUserId == tgId) ?? throw new Exception($"No User with {tgId} tg id");
        }
    }
}
