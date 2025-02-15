using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.SubscriptionsQuery.Dto;

namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly ProductRepository<Product> _productRepository;
        private readonly SubscriptionRepository<Subscription> _subscriptionRepository;
        private readonly UserRepository<User> _userRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        private readonly ILogger<SubscriptionsQuerys> _logger;

        public SubscriptionsQuerys(ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IRegionFromCookie regionFromCookie,
            ILogger<SubscriptionsQuerys> logger)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;
        }

        /// <summary>
        /// Выдача списка подписок
        /// </summary>
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList()
        {
            try
            {
                string region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                var subscriptions = await _subscriptionRepository.GetAllList();

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

                var currentSub = await _subscriptionRepository.GetById(Id)
                    ?? throw new KeyNotFoundException($"Subscription with ID {Id} not found");

                var subs = await _subscriptionRepository.SubscriptionsByName(currentSub.Name);
                var prod = await _productRepository.GetEntityType(Id);

                var price = await _calculatePrice.CalcPrice(prod.PriceUa, prod.PriceTr, prod.Type, region);
                var jPrice = await _calculatePrice.CalcJprice(price, region);
                var jPlus = await _calculatePrice.CalcJplus(jPrice);

                var userTg = _regionFromCookie.GetUserTgID(_httpContextAccessor);
                var user = await _userRepository.GetUserByTgId(userTg);

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
