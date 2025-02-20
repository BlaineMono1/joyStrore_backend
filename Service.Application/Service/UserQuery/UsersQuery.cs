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
        private readonly ProductRepository<Product> _productRepository;
        private readonly Repository<CartItem> _cartItemRepository;
        private readonly Repository<FavoriteItem> _favoriteItemRepository;
        private readonly Repository<Favorite> _favoriteRepository;
        private readonly Repository<Cart> _cartRepository;

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
                    return new UserDto();
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
                    Platform = user.Platform
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
                    var result = new CartItemDto();

                    switch (item.Product.Type)
                    {
                        case "Game":
                            result.image = item.Product.Edition.Image;
                            result.Name = item.Product.Edition.EditionName;
                            result.EditionName = item.Product.Edition.EditionType;
                            result.Id = item.Product.Edition.Guid;
                            result.Discount = item.Product.DiscountPercent;
                            result.Price = price;
                            result.JPrice = jPrice;
                            result.Platform = item.Product.Edition.Platform;
                            break;
                        case "AddOn":
                            result.image = item.Product.AddOn.Image;
                            result.Name = item.Product.AddOn.Name;
                            result.EditionName = "";
                            result.Id = item.Product.AddOn.Guid;
                            result.Discount = item.Product.DiscountPercent;
                            result.Price = price;
                            result.JPrice = jPrice;
                            result.Platform = item.Product.AddOn.Platform;
                            break;
                        case "Subscription":
                            result.image = item.Product.Subscription.Image;
                            result.Name = item.Product.Subscription.Name;
                            result.EditionName = "";
                            result.Id = item.Product.Subscription.Guid;
                            result.Discount = item.Product.DiscountPercent;
                            result.Price = price;
                            result.JPrice = jPrice;
                            result.Platform = item.Product.Subscription.Platform;
                            break;
                    }


                    return result;
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
                    var result = new FavoriteDto();
                    switch (item.Product.Type)
                    {
                        case "Game":
                            result.image = item.Product.Edition.Image;
                            result.Name = item.Product.Edition.EditionName;
                            result.EditionName = item.Product.Edition.EditionType;
                            result.Id = item.Product.Edition.Guid;
                            result.Discount = item.Product.DiscountPercent;
                            result.Price = price;
                            result.JPrice = jPrice;
                            result.DiscountTime = item.Product.DiscountDate;
                            break;
                        case "AddOn":
                            result.image = item.Product.AddOn.Image;
                            result.Name = item.Product.AddOn.Name;
                            result.EditionName = "";
                            result.Id = item.Product.AddOn.Guid;
                            result.Discount = item.Product.DiscountPercent;
                            result.Price = price;
                            result.JPrice = jPrice;
                            result.DiscountTime = item.Product.DiscountDate;
                            break;
                        case "Subscription":
                            result.image = item.Product.Subscription.Image;
                            result.Name = item.Product.Subscription.Name;
                            result.EditionName = "";
                            result.Id = item.Product.Subscription.Guid;
                            result.Discount = item.Product.DiscountPercent;
                            result.Price = price;
                            result.JPrice = jPrice;
                            result.DiscountTime = item.Product.DiscountDate;
                            break;
                    }
                    result.InCart = (user.Cart.CartItems is null) ? false : user.Cart.CartItems.Any(c => c.ProductId == item.ProductId);

                    return result;
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
                        items = (List<CartItemDto>)orderItem.OrderProductItems.Select(item =>
                        {
                            var result = new CartItemDto();
                            switch (item.Product.Type)
                            {
                                case "Game":
                                    result.image = item.Product.Edition.Image;
                                    result.Name = item.Product.Edition.EditionName;
                                    result.EditionName = item.Product.Edition.EditionType;
                                    result.Id = item.Product.Edition.Guid;
                                    result.Discount = item.Product.DiscountPercent;
                                    result.Price = item.Pirce;
                                    break;
                                case "AddOn":
                                    result.image = item.Product.AddOn.Image;
                                    result.Name = item.Product.AddOn.Name;
                                    result.EditionName = "";
                                    result.Id = item.Product.AddOn.Guid;
                                    result.Discount = item.Product.DiscountPercent;
                                    result.Price = item.Pirce;
                                    break;
                                case "Subscription":
                                    result.image = item.Product.Subscription.Image;
                                    result.Name = item.Product.Subscription.Name;
                                    result.EditionName = "";
                                    result.Id = item.Product.Subscription.Guid;
                                    result.Discount = item.Product.DiscountPercent;
                                    result.Price = item.Pirce;
                                    break;
                            }
                            return result;
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

        public async Task UpdateUserCart(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation($"Updating user {tgId} cart: {itemId}");

                var product = await  _productRepository.GetEntityType(itemId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", itemId);
                    throw new Exception($"Product with GUID {itemId} not found");
                }
                
                var cart = (await _userRepository.GetUserByTgId(tgId)).Cart;
                if (cart is null)
                {
                    _logger.LogError("User Cart with tg id {id} not found", tgId);
                    throw new Exception($"User Cart with tg id {tgId} not found");
                }
                var result = new CartItem()
                {
                    CartId = cart.Guid,
                    Cart = cart,
                    ProductId = product.Guid,
                    Product = product
                };
                
                await _cartItemRepository.Add(result);
                
                cart.CartItems ??= new List<CartItem>();
                cart.CartItems.Add(result);
                await _cartRepository.Update(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user cart tg Id {tgid}, itemId {itemId}", tgId, itemId);
                throw;
            }
        }

        public async Task UpdateUserFavorites(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation($"Updating user {tgId} favorites: {itemId}");

                var product = await _productRepository.GetEntityType(itemId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", itemId);
                    throw new Exception($"Product with GUID {itemId} not found");
                }

                var fav = (await _userRepository.GetUserByTgId(tgId)).Favorite;
                if (fav is null)
                {
                    _logger.LogError("User Favorite with tg id {id} not found", tgId);
                    throw new Exception($"User Favorite with tg id {tgId} not found");
                }
                var result = new FavoriteItem()
                {
                    ProductId = product.Guid,
                    Product = product
                };

                await _favoriteItemRepository.Add(result);

                fav.FavoriteItems ??= new List<FavoriteItem>();
                fav.FavoriteItems.Add(result);
                await _favoriteRepository.Update(fav);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user favorite tg Id {tgid}, itemId {itemId}", tgId, itemId);
                throw;
            }
        }

        public async Task DeleteFromCart(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation($"Deliting from user {tgId} cart: {itemId}");

                var cart = (await _userRepository.GetUserByTgId(tgId)).Cart.CartItems;

                if (cart is null)
                {
                    _logger.LogError("User Cart with tg id {id} not found", tgId);
                    throw new Exception($"User Cart with tg id {tgId} not found");
                }

                var item = cart.FirstOrDefault(c => c.ProductId == itemId);
                if(item is null)
                {
                    _logger.LogError("CartItem with id {id} not found in User {id} Cart", itemId, tgId);
                    throw new Exception($"User Cart with tg id {tgId} not found");
                }
                await _cartItemRepository.HardDelete(item.Guid);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating user Cart tg Id {tgid}, itemId {itemId}", tgId, itemId);
                throw;
            }
        }

        public async Task DeleteFromFavorites(string tgId, Guid itemId)
        {
            try
            {
                _logger.LogInformation($"Deliting from user {tgId} favorites: {itemId}");

                var fav = (await _userRepository.GetUserByTgId(tgId)).Favorite.FavoriteItems;

                if (fav is null)
                {
                    _logger.LogError("User Favorites with tg id {id} not found", tgId);
                    throw new Exception($"User Favorites with tg id {tgId} not found");
                }

                var item = fav.FirstOrDefault(c => c.ProductId == itemId);
                if (item is null)
                {
                    _logger.LogError("FavoriteItem with id {id} not found in User {id} Favorites", itemId, tgId);
                    throw new Exception($"User FavoriteItem with tg id {tgId} not found");
                }
                await _favoriteRepository.HardDelete(item.Guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user favorite tg Id {tgid}, itemId {itemId}", tgId, itemId);
                throw;
            }
        }

        public async Task UpdateConsoleType(string tgId, string Console)
        {
            var user = await _userRepository.GetUserByTgId(tgId);

            user.Platform = Console;
            await _userRepository.Update(user);
        }
    }
}
