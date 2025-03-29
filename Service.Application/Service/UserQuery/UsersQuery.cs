using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.CartQuery.Dto;
using Service.Application.Service.UserQuery.Dto;


namespace Service.Application.Service.UserQuery
{
    public class UsersQuery
    {
        private readonly IUserRepository<User> _userRepository;
        private readonly IRepository<Setting> _setingsRepository;
        private readonly IRepository<LoyaltyCurrency> _loyalityRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<FavoriteItem> _favoriteItemRepository;
        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<Cart> _cartRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataFromCookie _regionFromCookie;
        private readonly ILogger<UsersQuery> _logger;

        public UsersQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IDataFromCookie regionFromCookie,
            ILogger<UsersQuery> logger,
            IUserRepository<User> userRepository,
            IRepository<Setting> setingsRepository,
            IRepository<LoyaltyCurrency> loyalityRepository,
            IProductRepository<Product> productRepository,
            IRepository<CartItem> cartItemRepository,
            IRepository<FavoriteItem> favoriteItemRepository,
            IRepository<Favorite> favoriteRepository,
            IRepository<Cart> cartRepository)


        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;
            _userRepository = userRepository;
            _setingsRepository = setingsRepository;
            _loyalityRepository = loyalityRepository;
            _productRepository = productRepository;
            _cartItemRepository = cartItemRepository;
            _favoriteItemRepository = favoriteItemRepository;
            _favoriteRepository = favoriteRepository;
            _cartRepository = cartRepository;
        }

        public async Task<UserDto> UserByTgId()
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion();
                var tgId = _regionFromCookie.GetUserTgID();

                _logger.LogInformation("Fetching user by TG ID: {TgId}", tgId);

                var user = await _userRepository.GetUserByTgId(tgId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for TG ID: {TgId}", tgId);
                    return new UserDto();
                }

                var settings = (await _setingsRepository.GetListQuery()).FirstOrDefault(s => s.UserId == user.Guid && s.Region == region);
                var loyaloty = await _loyalityRepository.GetById(user.LoyaltyCurrencyId);

                var result = new UserDto
                {
                    Id = user.Guid,
                    Email = settings?.EmailPsStore ?? "",
                    Password = settings?.PasswordPsStore ?? "",
                    Code = settings?.Code ?? "",
                    JBal = loyaloty?.BalanceJoy ?? 0,
                    JPlus = loyaloty?.BalanceJoyPlus ?? 0,
                    Platform = user.Platform
                };

                _logger.LogInformation("Successfully fetched user data for TG ID: {TgId}", tgId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }       
        
        public async Task<List<OrderDto>> UserOrder()
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion();
                var tgId = _regionFromCookie.GetUserTgID();
                _logger.LogInformation("Fetching user orders for TG ID: {TgId}", tgId);

                var user = (await _userRepository.GetListQuery()).Include(u => u.ProductTransactionHistory).ThenInclude(c => c.Orders).FirstOrDefault(u => u.TgUserId == tgId);
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
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task UpdateConsoleType(string Console)
        {
            try
            {
                var tgId = _regionFromCookie.GetUserTgID();
                var user = await _userRepository.GetUserByTgId(tgId);
                if (user == null)
                {
                    _logger.LogError("User not found for TG ID: {TgId}", tgId);
                    throw new Exception($"User not found for TG ID: {tgId}");
                }
                user.Platform = Console;
                await _userRepository.Update(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task UpdateUserSettings(string email, string password, string code)
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion();
                var tgId = _regionFromCookie.GetUserTgID();

                var userSettings = (await _userRepository.GetListQuery()).Include(u => u.Settings).First(u => u.TgUserId == tgId).Settings.FirstOrDefault(s => s.Region == region);

                if (userSettings is null) throw new KeyNotFoundException($"No user settings with user tgId {tgId}");

                userSettings.Code = code;
                userSettings.EmailPsStore = email;
                userSettings.PasswordPsStore = password;

                await _setingsRepository.Update(userSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task CreateUser(string tgId)
        {
            try
            {
                var user = await _userRepository.GetUserByTgId(tgId);
                if(user is null)
                {
                    var newUser = new User
                    {
                        TgUserId = tgId,
                        Platform = ""
                    };
                    var cart = new Cart
                    {
                        UserId = newUser.Guid,
                        User = newUser,
                    };
                    var fav = new Favorite
                    {
                        UserId = newUser.Guid,
                        User = newUser,
                    };
                    var settingUah = new Setting
                    {
                        UserId = newUser.Guid,
                        Region = "UAH",
                        User = newUser
                    };
                    var settingTRL = new Setting
                    {
                        UserId = newUser.Guid,
                        Region = "TRY",
                        User = newUser
                    };
                    var loyality = new LoyaltyCurrency{ User = newUser };
                    var history = new ProductTransactionHistory{ User = newUser };

                    var role = new Role()
                    {
                        Name = "User",
                        Users = new List<User> { newUser }
                    };
                    newUser.Settings = new List<Setting>{ settingTRL, settingUah};
                    newUser.Cart = cart;
                    newUser.Favorite = fav;
                    newUser.LoyaltyCurrency = loyality;
                    newUser.ProductTransactionHistory = history;
                    newUser.Role = role;
                    await _userRepository.Add(newUser);
                    
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
