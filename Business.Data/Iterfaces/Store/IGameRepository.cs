using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IGameRepository<T> : IRepository<Game> where T : class, IBaseEntity
    {
    }
}
