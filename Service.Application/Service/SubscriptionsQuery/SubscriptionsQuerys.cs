using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.SubscriptionsQuery.Dto;

namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly ISubscriptionRepository<Subscription> _subscriptionRepository;
        private readonly IUserRepository<User> _userRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        private readonly ILogger<SubscriptionsQuerys> _logger;

        public SubscriptionsQuerys(ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IRegionFromCookie regionFromCookie,
            ILogger<SubscriptionsQuerys> logger,
            IProductRepository<Product> productRepository,
            ISubscriptionRepository<Subscription> subscriptionRepository,
            IUserRepository<User> userRepository)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;

            _productRepository = productRepository;
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;

        }

        /// <summary>
        /// Выдача списка подписок
        /// </summary>
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList()
        {
            try
            {
                string region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                var subscriptions = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).ToList();

                _logger.LogInformation("Fetched {Count} subscriptions.", subscriptions.Count);

                var tasks = subscriptions.Select(async sub =>
                {
                    try
                    {
                        var product = await _productRepository.GetById(sub.ProductId)
                            ?? throw new KeyNotFoundException($"Product with TypeId {sub.Guid} not found");

                        var price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type, region);
                        var jPrice = await _calculatePrice.CalcJprice(price, region);

                        return new SubscriptionsListDto
                        {
                            id = sub.Guid,
                            Name = sub.Name,
                            ImagePath = sub.Image,
                            Dicount = product.DiscountPercent,
                            Price = price,
                            Jprice = jPrice
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing subscription {SubscriptionId}", sub.Guid);
                        return null;
                    }
                });

                var result = (await Task.WhenAll(tasks)).Where(t => t != null).ToList();
                _logger.LogInformation("Successfully processed {Count} subscriptions.", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscriptions list.");
                return new List<SubscriptionsListDto>();
            }
        }

        /// <summary>
        /// Получение подписки по ID
        /// </summary>
        public async Task<SubscriptionDto> SubscriptionById(Guid Id)
        {
            try
            {
                string region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                _logger.LogInformation("Fetching subscription details for ID: {Id}", Id);

                var currentSub = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).FirstOrDefault(s => s.Guid == Id)
                    ?? throw new KeyNotFoundException($"Subscription with ID {Id} not found");

                var subs = await _subscriptionRepository.SubscriptionsByName(currentSub.Name);
                var prod = await _productRepository.GetEntityType(Id);

                var price = await _calculatePrice.CalcPrice(prod.PriceUa, prod.PriceTr, prod.Type, region);
                var jPrice = await _calculatePrice.CalcJprice(price, region);
                var jPlus = await _calculatePrice.CalcJplus(jPrice);

                var userTg = _regionFromCookie.GetUserTgID(_httpContextAccessor);
                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).Include(u => u.Favorite).ThenInclude(f => f.FavoriteItems).Include(u => u.Settings).FirstOrDefault(u => u.TgUserId == userTg);

                var result = new SubscriptionDto
                {
                    Id = Id,
                    Image = currentSub.Image,
                    Type = prod.Type,
                    Platform = currentSub.Platform,
                    Subscriptions = subs,
                    Discount = prod.DiscountPercent,
                    Price = price,
                    JPrice = jPrice,
                    JPlus = jPlus,
                    InCart = user.Cart.CartItems.Any(c => c.ProductId == currentSub.Product.Guid),
                    InFavorite = user.Favorite.FavoriteItems.Any(c => c.ProductId == currentSub.Product.Guid)
                };

                _logger.LogInformation("Successfully fetched subscription details for ID: {Id}", Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscription details for ID: {Id}", Id);
                throw;
            }
        }
    }
}
