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
                .Include(u => u.Cart).ThenInclude(u => u.CartItems).ThenInclude(u => u.Product).ThenInclude(u => u.Edition)
                .Include(u => u.Favorite).ThenInclude(u => u.FavoriteItems).ThenInclude(u => u.Product).ThenInclude(u => u.Edition)
                .Include(u => u.ProductTransactionHistory).ThenInclude(u => u.Orders).ThenInclude(u => u.OrderProductItems).ThenInclude(u => u.Product).ThenInclude(u => u.Edition)
                .FirstOrDefault(u => u.TgUserId == tgId) ?? throw new Exception($"No User with {tgId} tg id");

        }
    }
}
