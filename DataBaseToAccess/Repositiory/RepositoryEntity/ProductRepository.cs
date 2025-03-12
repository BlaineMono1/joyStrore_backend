using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;


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

        public async Task<IQueryable<Product>> FilterProducts(string? name, List<string>? FilterGeners)
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

            var games =  filteredByGener.Include(p => p.Edition).ThenInclude(e => e.Game).ThenInclude(g => g.AddOns);

            var set = new HashSet<string>();
            foreach (var game in games)
            {
                set.Add(game.Edition.Game.Name);
            }

            var result = (await GetListQuery()).Where(p => (p.Edition != null && set.Contains(p.Edition.Game.Name)) || (p.AddOn != null && set.Contains(p.AddOn.Game.Name)))
                .Include(p => p.Edition)
                .Include(p => p.AddOn);

            return result;
        }
    }
}
