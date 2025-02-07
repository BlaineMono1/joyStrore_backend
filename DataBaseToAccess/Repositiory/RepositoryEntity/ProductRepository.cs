using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;


namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class ProductRepository<T> : Repository<Product>, IProductRepository<T> where T : class,IBaseEntity
    {
        public ProductRepository(BaseDbContext contex) : base(contex) { }

        private readonly Repository<Edition> _editionRepository;
        private readonly Repository<AddOn> _addOnRepository;
        private readonly Repository<Subscription> _subscriptionRepository;
        public async Task<T> GetTypeEntity(Product product)
        {
            if (product == null)
                throw new KeyNotFoundException("Entity not found.");

            object? result = null;

            switch (product.Type)
            {
                case "Edition":
                    result = await _editionRepository.GetById(product.TypeId);
                    break;
                case "AddOn":
                    result = await _addOnRepository.GetById(product.TypeId);
                    break;
                case "Subscription":
                    result = await _subscriptionRepository.GetById(product.TypeId);
                    break;
                default:
                    throw new KeyNotFoundException($"Type '{product.Type}' is not found.");
            }

            return result as T ?? throw new InvalidCastException($"Cannot convert {product.Type} to {typeof(T)}.");
        }

        public async Task<T> GetEntityType(Guid id)
        {
            object? result = (await GetAllList()).FirstOrDefault(e => e.TypeId == id);
            return result == null
                ? throw new KeyNotFoundException($"Produnct with type id: {id} not found")
                : result as T ?? throw new InvalidCastException($"Cannot convert {result} to {typeof(T)}.");
            ;
        }
    }
}
