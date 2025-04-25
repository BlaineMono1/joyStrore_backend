using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.ProductQuery.Dto;

namespace Service.Application.Service.ProductQuery
{
    public class ProductQuery
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly IEditionRepository<Edition> _editonRepository;
        private readonly IRepository<AddOn> _addOnRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IUserRepository<User> _userRepository;
        private readonly IRepository<Game> _gameRepository;

        private readonly IRedisRepository _redis;
        private readonly ICacheService _cacheService;

        private readonly IDataFromCookie _regionFromCookie;
        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<ProductQuery> _logger;

        public ProductQuery(IProductRepository<Product> productRepository,
            IEditionRepository<Edition> editonRepository,
            IRepository<AddOn> addOnRepository,
            IRepository<Subscription> subscriptionRepository,
            IUserRepository<User> userRepository,
            IHttpContextAccessor httpContextAccessor,
            IDataFromCookie regionFromCookie,
            ICalculationService calculatePrice,
            ILogger<ProductQuery> logger,
            IRepository<Game> gameRepository,
            IRedisRepository redis,
            ICacheService cacheService)
        {
            _productRepository = productRepository;
            _editonRepository = editonRepository;
            _addOnRepository = addOnRepository;
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;

            _regionFromCookie = regionFromCookie;
            _calculatePrice = calculatePrice;
            _logger = logger;
            _gameRepository = gameRepository;
            _redis = redis;
            _cacheService = cacheService;
        }
        public async Task<ProductDto> GetProduct(Guid ProductId)
        {
            var result = new ProductDto();
            try
            {
                var product = await _productRepository.GetById(ProductId);
                result.ProductId = ProductId;
                result.ProductType = product.Type;
                result.Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price);
                result.JPlus = await _calculatePrice.CalcJplus(result.JPrice);
                result.Discount = product.DiscountDate;
                result.DiscountPercent = product.DiscountPercent;
                var userTg = _regionFromCookie.GetUserTgID();
                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).Include(u => u.Favorite).ThenInclude(f => f.FavoriteItems)
                    .FirstOrDefault(u => u.TgUserId == userTg);

                result.InCart = user.Cart.CartItems.Any(c => c.ProductId == product.Guid);
                result.InFavorite = user.Favorite.FavoriteItems.Any(c => c.ProductId == product.Guid);
                

                switch (product.Type)
                {
                    case "Game":

                        var edition = (await _editonRepository.GetListQuery()).Include(e => e.Game)
                            .Include(e => e.EditionGeners).ThenInclude(eg => eg.Geners).FirstOrDefault(e => e.Guid == product.TypeId);
                        result.Image = edition.Image;
                        result.Geners = edition.EditionGeners.Select(eg => eg.Geners.Name).ToList();
                        result.RealiseDate = edition.Release;
                        result.Platforms = edition.Platform;
                        result.Languages = edition.Game.Languages;
                        result.Subscription = edition.Subscription;
                        result.Features = edition.Features;
                        break;

                    case "AddOn":

                        var addOn = await _addOnRepository.GetById(product.TypeId);
                        result.Image = addOn.Image;
                        result.Platforms = addOn.Platform;
                        break;

                    case "Subscription":

                        var sub = await _subscriptionRepository.GetById(product.TypeId);
                        result.Image = sub.Image;
                        result.Platforms = sub.Platform;
                        break;
                }
                result.IsPlatform = result.Platforms.Contains(user.Platform);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<List<DropDownListDto>> DropDownList(Guid ProductId)
        {
            var result = new List<DropDownListDto>();
            try
            {
                var product = await _productRepository.GetById(ProductId);
                switch (product.Type)
                {
                    case "Game":
                        var edition = await _editonRepository.GetById(product.TypeId);
                        var editions = (await _gameRepository.GetListQuery()).Include(g => g.Editions).FirstOrDefault(g => g.Editions.Contains(edition)).Editions;

                        result.AddRange(await Task.WhenAll(
                            editions.Where(e => e.Guid != product.TypeId).Select(
                                async item =>
                                new DropDownListDto
                                {
                                    Name = item.Name,
                                    ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid
                                })));

                        break;
                    case "AddOn":

                        var addOn = (await _addOnRepository.GetListQuery()).Include(a => a.Game).ThenInclude(g => g.AddOns).FirstOrDefault(a => a.Guid == product.TypeId);

                        result.AddRange(await Task.WhenAll(
                            addOn.Game.AddOns.Where(a => a.Guid != product.TypeId).Select(
                                async item =>
                                new DropDownListDto
                                {
                                    Name = item.Name,
                                    ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid
                                })));

                        break;

                    case "Subscription":

                        var sub = (await _subscriptionRepository.GetListQuery()).FirstOrDefault(s => s.Guid == product.TypeId); //текущая подписка
                        var groupSubs = (await _subscriptionRepository.GetListQuery()).Where(s => s.Name == sub.Name && s.Guid != sub.Guid).ToList();

                        result.AddRange(await Task.WhenAll(
                            groupSubs.Select(async item =>
                                 new DropDownListDto
                                 { 
                                     Name = item.Duration,
                                     ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid
                                 }

                            )));

                        break;

                }



                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }


        public async Task<IQueryable<Product>> FilterProducts(string? name, string? filterName, string? platform, bool byDesc, bool byDiscount, List<string>? FilterGeners, decimal MinPrice, decimal MaxPrice)
        {
            var products = (await _productRepository.GetListQuery()).Where(p => p.Type == "Game");

            var filteredByName = products;
            if (!string.IsNullOrEmpty(name))
            {
                filteredByName = products.Where(p => p.Edition.Name.ToLower().Contains(name.ToLower()));

            }

            var filteredByGener = filteredByName;

            if (FilterGeners != null && FilterGeners.Any())
            {
                filteredByGener = filteredByName.Where(p => p.Edition.EditionGeners.Any(g => FilterGeners.Contains(g.Geners.Name)));

            }

            var games = filteredByGener.Include(p => p.Edition).ThenInclude(e => e.Game);

            var set = games.Select(p => p.Edition.Game.Guid).ToHashSet();

            var result = (await _productRepository.GetListQuery()).Include(p => p.Edition).ThenInclude(e => e.Game).Include(p => p.AddOn).ThenInclude(a => a.Game).Where(p => (p.Type == "Game" && set.Contains(p.Edition.Game.Guid)) || (p.Type == "AddOn" && set.Contains(p.AddOn.Game.Guid)));

            if (!string.IsNullOrEmpty(filterName))
            {
                switch (filterName)
                {
                    case "Date":
                        result = byDesc ? result.OrderByDescending(p => p.Type == "Game" ? p.Edition.Release : DateTime.MaxValue) : result.OrderBy(p => p.Type == "Game" ? p.Edition.Release : DateTime.MinValue);
                        break;
                    case "Price":
                        result = byDesc ? result.OrderByDescending(p => p.PriceUa) : result.OrderBy(p => p.PriceUa);
                        break;
                    default:
                        result = result.OrderByDescending(p => p.Type == "Game" ? p.Edition.Game.Popular : p.AddOn.Game.Popular);
                        break;
                }

            }

            if (!string.IsNullOrEmpty(platform)) result = result.Where(p => p.Type == "Game" ? p.Edition.Platform.Contains(platform) : p.AddOn.Platform.Contains(platform));

            if (byDiscount)
            {
                result = result.OrderByDescending(p => p.DiscountPercent ?? "0");
            }

            var region = _regionFromCookie.GetUserRegion();

            string? cachedData = await _redis.GetAsync(region);
            if (cachedData is null)
            {
                await _cacheService.UpdateExchangeRates();
                cachedData = await _redis.GetAsync(region);
            }

            decimal coff = decimal.Parse(cachedData);

            result = result.Where(p => region == "UAH" ? p.PriceUa * coff >= MinPrice && p.PriceUa * coff <= MaxPrice : p.PriceTr * coff >= MinPrice && p.PriceTr * coff <= MaxPrice);

            return result;
        }

        public async Task<List<ProductListDto>> MapProducts(IEnumerable<Product> source)
        {
           var result = new List<ProductListDto>();
            foreach(var item in source)
            {
                var t = new ProductListDto();
                t.ProductId = item.Guid;
                t.ImageFilepath = item.Type == "Game" ? (await _editonRepository.GetById(item.TypeId)).Image : (await _addOnRepository.GetById(item.TypeId)).Image;
                t.Name = item.Type == "Game" ? (await _editonRepository.GetById(item.TypeId)).Name : (await _addOnRepository.GetById(item.TypeId)).Name;
                t.Price = await _calculatePrice.CalcPrice(item.PriceUa, item.PriceTr, item.Type);
                t.Jprice = await _calculatePrice.CalcJprice(t.Price);
                t.Discount = item.DiscountPercent;
                result.Add(t);
            }
            
            return result;

        }
    }
}