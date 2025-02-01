using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using System.Data;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class ProductRepository<T> : Repository<Product>, IProductRepository<T> where T : class,IBaseEntity
    {
        public ProductRepository(BaseDbContext contex) : base(contex) { }

        private readonly IGameRepository<T> _gameRepository;
        private readonly IAddOnRepository<T> _addOnRepository;
        private readonly ISubscriptionRepository<T> _subscriptionRepository;
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
                    result = (await _addOnRepository.GetAllList()).FirstOrDefault(g => g.Guid == product.TypeId);
                    break;
                case "Subscription":
                    result = (await _subscriptionRepository.GetAllList()).FirstOrDefault(g => g.Guid == product.TypeId);
                    break;
                default:
                    throw new KeyNotFoundException($"Type '{product.Type}' is not found.");
            }

            return result as T ?? throw new InvalidCastException($"Cannot convert {product.Type} to {typeof(T)}.");
        }
    }
}
