using Business.Data.BaseEntities;

namespace Business.Data.Iterfaces
{
    public interface IRepository<T> where T : IBaseEntity
    {
        Task<List<T>> GetAllList();

    }
}
