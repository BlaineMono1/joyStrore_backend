using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IUserRepository<T> : IRepository<User> where T : class, IBaseEntity
    {
        Task<User> GetUserByTgId(string tgId);
    }
}
