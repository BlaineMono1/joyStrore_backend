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
        public ProductRepository(BaseDbContext contex, IRedisRepository redis) : base(contex)
        {
            _redis = redis;
        }

        private readonly Repository<Edition> _editionRepository;
        private readonly Repository<AddOn> _addOnRepository;
        private readonly Repository<Subscription> _subscriptionRepository;

        private readonly IRedisRepository _redis;
        public async Task<T> GetTypeEntity(Product product)
        {
            if (product == null)
                throw new KeyNotFoundException("Entity not found.");

            object? result = null;

            switch (product.Type)
            {
                case "Game":
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
            object? result = (await GetListQuery()).FirstOrDefault(e => e.TypeId == id);
            return result == null
                ? throw new KeyNotFoundException($"Produnct with type id: {id} not found")
                : result as T ?? throw new InvalidCastException($"Cannot convert {result} to {typeof(T)}.");
            ;
        }

        public async Task<IQueryable<Product>> FilterProducts(string? name, string? filterName, string? platform, bool byDesc, bool byDiscount, List<string>? FilterGeners)
        {
            var products = (await GetListQuery()).Where(p => p.Type == "Game");

            var filteredByName = products;
            if (!string.IsNullOrEmpty(name))
            {
                filteredByName = products.Where(p => p.Edition.EditionName.ToLower().Contains(name.ToLower()));
                    
            }

            var filteredByGener = filteredByName;

            if (FilterGeners != null && FilterGeners.Any())
            {
                filteredByGener = filteredByName.Where(p => p.Edition.EditionGeners.Any(g => FilterGeners.Contains(g.Geners.Name)));                   
                    
            }

            var games =  filteredByGener.Include(p => p.Edition).ThenInclude(e => e.Game);

            var set = games.Select(p => p.Edition.Game.Guid).ToHashSet();

            var result = (await GetListQuery()).Where(p => (p.Type == "Game" && set.Contains(p.Edition.Game.Guid)) || (p.Type == "AddOn" && set.Contains(p.AddOn.Game.Guid)));

            if(!string.IsNullOrEmpty(filterName))
            {
                switch (filterName)
                {
                    case "Date":
                        result = byDesc ? result.OrderByDescending(p => p.Type == "Game" ? p.Edition.Release : DateTime.MaxValue) : result.OrderBy(p => p.Type == "Game" ? p.Edition.Release : DateTime.MinValue);
                        break;
                    case "Price":
                        result = byDesc ? result.OrderByDescending(p => p.PriceUa) : result.OrderBy(p => p.PriceTr);
                        break;
                   
                }

            }           

            if (!string.IsNullOrEmpty(platform)) result = result.Where(p => p.Type == "Game" ? p.Edition.Platform.Contains(platform) : p.AddOn.Platform.Contains(platform));

            result = result.OrderByDescending(p => p.Type == "Game" ? p.Edition.Game.Popular : p.AddOn.Game.Popular);

            if (byDiscount)
            {
                result = result.OrderByDescending(p => p.DiscountPercent ?? "0");
            }            

            return result;
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
