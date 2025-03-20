using Business.Data.Models;

namespace Business.Data.Iterfaces.Store
{
    public interface IProductRepository<T>:IRepository<Product> where T : class,IBaseEntity
    {
        /// <summary>
        /// Получить объект(игру/подписку/аддон) через продукт
        /// </summary>
        /// <returns></returns>
        Task<T> GetTypeEntity(Product product);

        /// <summary>
        /// Получить продукт через объект(игру/подписку/аддон)
        /// </summary>
        /// <returns></returns>
        Task<T> GetEntityType(Guid id);

        Task<IQueryable<Product>> FilterProducts(string? name, string? filterName, string? platform, bool byDesc, bool byDiscount, List<string>? geners);

        new Task Update(Product entity);
    }
}
