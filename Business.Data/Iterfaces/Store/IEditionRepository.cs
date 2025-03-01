using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IEditionRepository<T>:IRepository<Edition> where T : class, IBaseEntity
    {
        Task<List<Edition>> GetEditions(Guid GameID);

        Task<List<Edition>> FilterEditions(string? name, List<string>? Geners);
    }
}
