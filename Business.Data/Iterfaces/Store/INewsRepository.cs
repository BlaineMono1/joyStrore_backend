using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface INewsRepository<T> : IRepository<News> where T : class, IBaseEntity
    {

    }
}
