using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IProductRepository<T>:IRepository<Product> where T : class,IBaseEntity
    {
        /// <summary>
        /// Получить объект(игру/подписку/аддон) через продукт
        /// </summary>
        /// <returns></returns>
        Task<T> GetTypeEntity<T>(Product product);

        /// <summary>
        /// Получить продукт через объект(игру/подписку/аддон)
        /// </summary>
        /// <returns></returns>
        Task<T> GetEntityType(Guid id);

        
        new Task Update(Product entity);
    }
}
