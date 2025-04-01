using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;


namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class ProductRepository<T> : Repository<Product>, IProductRepository<T> where T : class,IBaseEntity
    {
        private readonly Repository<Edition> _editionRepository;
        private readonly Repository<AddOn> _addOnRepository;
        private readonly Repository<Subscription> _subscriptionRepository;
        private readonly BaseDbContext _contex;
        public ProductRepository(BaseDbContext contex, IRedisRepository redis) : base(contex)
        {
            _redis = redis;
            _contex = contex;
           
        }

       

        private readonly IRedisRepository _redis;
        public async Task<T> GetTypeEntity<T>(Product product)
        {
            if (product == null)
                throw new KeyNotFoundException("Entity not found.");

            object? result = null;

            switch (product.Type)
            {
                case "Game":
                    result = await _contex.Editions.FindAsync(product.TypeId);
                    break;
                case "AddOn":
                    result = await _contex.AddOns.FindAsync(product.TypeId);
                    break;
                case "Subscription":
                    result = await _contex.Subscriptions.FindAsync(product.TypeId);
                    break;
                default:
                    throw new KeyNotFoundException($"Type '{product.Type}' is not found.");
            }

            return (T)result;
        }

        public async Task<T> GetEntityType(Guid id)
        {
            object? result = (await GetListQuery()).FirstOrDefault(e => e.TypeId == id);
            return result == null
                ? throw new KeyNotFoundException($"Produnct with type id: {id} not found")
                : result as T ?? throw new InvalidCastException($"Cannot convert {result} to {typeof(T)}.");
            
        }        

        public async new Task Update(Product entity)
        {
            await base.Update(entity);

            string cacheKey = $"product-{entity.Guid}";
            string jsonData = JsonSerializer.Serialize(entity);

            await _redis.SetAsync(cacheKey, jsonData, null);
        }
    }
}
