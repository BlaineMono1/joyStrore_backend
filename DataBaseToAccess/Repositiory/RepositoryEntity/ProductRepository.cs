using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;


namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class ProductRepository<T> : Repository<Product>, IProductRepository<T> where T : class,IBaseEntity
    {
        public ProductRepository(BaseDbContext contex) : base(contex) { }

        private readonly Repository<Game> _gameRepository;
        private readonly Repository<AddOn> _addOnRepository;
        private readonly Repository<Subscription> _subscriptionRepository;
        public async Task<T> GetTypeEntity(Guid id)
        {
            var product = (await GetAllList()).FirstOrDefault(p => p.Guid == id);
            if (product == null)
                throw new KeyNotFoundException("Entity not found.");

            object result = null;

            switch (product.Type)
            {
                case "Game":
                    result = (await _gameRepository.GetAllList()).FirstOrDefault(g => g.Guid == product.TypeId);
                    break;
                case "AddOn":
                    result = (await _addOnRepository.GetAllList()).FirstOrDefault(a => a.Guid == product.TypeId);
                    break;
                case "Subscription":
                    result = (await _subscriptionRepository.GetAllList()).FirstOrDefault(s => s.Guid == product.TypeId);
                    break;
                default:
                    throw new KeyNotFoundException($"Type '{product.Type}' is not found.");
            }

            return result as T ?? throw new InvalidCastException($"Cannot convert {product.Type} to {typeof(T)}.");
        }
    }
}
