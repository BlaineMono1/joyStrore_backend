using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IGameRepository<T> : IRepository<Game> where T : class
    {
        Task<List<AddOn>> AddOnsByGame(Guid AddOnId);

        Task<List<Game>> FilterGames(string name, List<string> Geners);
    }
}
