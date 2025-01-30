using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class ProductRepository<T> : Repository<Product>, IProductRepository<T> where T : class,IBaseEntity
    {
        public ProductRepository(BaseDbContext contex) : base(contex) { }

        public async Task<T> GetTypeEntity()
        {
            throw new NotImplementedException();
        }
    }
}
