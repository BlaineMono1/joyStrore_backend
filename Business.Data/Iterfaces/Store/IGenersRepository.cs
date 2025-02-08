using Business.Data.Models;


namespace Business.Data.Iterfaces.Store
{
    public interface IGenersRepository<T>: IRepository<Geners> where T : class
    {
        /// <summary>
        /// Получить список жанров
        /// </summary>
        /// <returns></returns>

        Task<List<string>> GetGeners(Guid EditionId);
        
    }
}
