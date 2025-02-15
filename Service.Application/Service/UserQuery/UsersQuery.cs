using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.UserQuery.Dto;

namespace Service.Application.Service.UserQuery
{
    public class UsersQuery
    {
        private readonly UserRepository<User> _userRepository;
        private readonly Repository<Setting> _setingsRepository;
        private readonly Repository<LoyaltyCurrency> _loyalityRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        private readonly ILogger<UsersQuery> _logger;

        public UsersQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IRegionFromCookie regionFromCookie,
            ILogger<UsersQuery> logger)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;
        }

        public async Task<UserDto> UserByTgId(string tgId)
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                _logger.LogInformation("Fetching user by TG ID: {TgId}", tgId);

                var user = await _userRepository.GetUserByTgId(tgId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for TG ID: {TgId}", tgId);
                    return null;
                }

                var settings = await _setingsRepository.GetById(user.Settings.FirstOrDefault(s => s.Region == region).Guid);
                var loyaloty = await _loyalityRepository.GetById(user.LoyaltyCurrencyId);

                var result = new UserDto
                {
                    Id = user.Guid,
                    Email = settings?.EmailPsStore,
                    Password = settings?.PasswordPsStore,
                    Code = settings?.Code,
                    JBal = loyaloty?.BalanceJoy ?? 0,
                    JPlus = loyaloty?.BalanceJoyPlus ?? 0,
                    Platform = settings?.Platform
                };

                _logger.LogInformation("Successfully fetched user data for TG ID: {TgId}", tgId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by TG ID: {TgId}", tgId);
                throw;
            }
        }

        public async Task<CartDto> UserCart(string tgId)
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                _logger.LogInformation("Fetching user cart for TG ID: {TgId}", tgId);

                var user = await _userRepository.GetUserByTgId(tgId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for TG ID: {TgId}", tgId);
                    return new CartDto();
                }

                var userCartItems = user.Cart.CartItems?.Where(item => !item.IsDelete) ?? Enumerable.Empty<CartItem>();

                var cart = await Task.WhenAll(userCartItems.Select(async item =>
                {
                    var price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type, region);
                    var jPrice = await _calculatePrice.CalcJprice(price, region);

                    return new CartItemDto
                    {
                        image = item.Product.Edition.Image,
                        Name = item.Product.Edition.Game.Name,
                        EditionName = item.Product.Edition.EditionName,
                        GameId = item.Product.Edition.GameId,
                        Discount = item.Product.DiscountPercent,
                        Price = price,
                        JPrice = jPrice,
                        Platform = item.Product.Edition.Platform
                    };
                }));

                var settings = await _setingsRepository.GetById(user.Settings.FirstOrDefault(s => s.Region == region).Guid);

                var result = new CartDto
                {
                    items = cart.ToList(),
                    Email = settings?.EmailPsStore,
                    PayEmail = settings?.Email,
                    Password = settings?.PasswordPsStore,
                    Code = settings?.Code
                };

                _logger.LogInformation("Successfully fetched user cart for TG ID: {TgId}", tgId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user cart for TG ID: {TgId}", tgId);
                throw;
            }
        }

        public async Task<List<FavoriteDto>> UserFavorite(string TgId)
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                _logger.LogInformation("Fetching user favorite items for TG ID: {TgId}", TgId);

                var user = await _userRepository.GetUserByTgId(TgId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for TG ID: {TgId}", TgId);
                    return new List<FavoriteDto>();
                }

                var favoriteItems = user.Favorite.FavoriteItems?.Where(item => !item.IsDelete) ?? Enumerable.Empty<FavoriteItem>();

                var result = await Task.WhenAll(favoriteItems.Select(async item =>
                {
                    var price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type, region);
                    var jPrice = await _calculatePrice.CalcJprice(price, region);

                    return new FavoriteDto
                    {
                        GameId = item.Product.Edition.GameId,
                        Image = item.Product.Edition.Image,
                        Name = item.Product.Edition.Game.Name,
                        Edition = item.Product.Edition.EditionName,
                        Discount = item.Product.DiscountPercent,
                        Price = price,
                        JPrice = jPrice,
                        InCart = user.Cart.CartItems.Any(c => c.ProductId == item.ProductId)
                    };
                }));

                _logger.LogInformation("Successfully fetched user favorite items for TG ID: {TgId}", TgId);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user favorite items for TG ID: {TgId}", TgId);
                throw;
            }
        }

        public async Task<List<OrderDto>> UserOrder(string tgId)
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                _logger.LogInformation("Fetching user orders for TG ID: {TgId}", tgId);

                var user = await _userRepository.GetUserByTgId(tgId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for TG ID: {TgId}", tgId);
                    return new List<OrderDto>();
                }

                var orders = user.ProductTransactionHistory.Orders?.Where(order => !order.IsDelete) ?? Enumerable.Empty<Order>();

                var result = await Task.WhenAll(orders.Select(async orderItem =>
                {
                    var orderDto = new OrderDto
                    {
                        OrderNumber = orderItem.OrderCode,
                        OrderDate = orderItem.DateCreate,
                        items = (List<CartItemDto>)orderItem.OrderProductItems.Select(productOrderItem =>
                        {
                            return new CartItemDto
                            {
                                GameId = productOrderItem.Product.Edition.GameId,
                                image = productOrderItem.Product.Edition.Image,
                                Name = productOrderItem.Product.Edition.Game.Name,
                                EditionName = productOrderItem.Product.Edition.EditionName,
                                Price = productOrderItem.Pirce,
                                Discount = productOrderItem.Discount,
                                Platform = productOrderItem.Product.Edition.Platform
                            };
                        })
                    };

                    return orderDto;
                }));

                _logger.LogInformation("Successfully fetched user orders for TG ID: {TgId}", tgId);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user orders for TG ID: {TgId}", tgId);
                throw;
            }
        }
    }
}
