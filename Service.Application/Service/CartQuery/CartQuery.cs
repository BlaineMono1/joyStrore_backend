using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.CartQuery.Dto;

namespace Service.Application.Service.CartQuery
{
    public class CartQuery
    {
        private readonly IUserRepository<User> _userRepository;
        private readonly IRepository<Setting> _setingsRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IRepository<CartItem> _cartItemRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IDataFromCookie _regionFromCookie;
        private readonly ILogger<CartQuery> _logger;

        public CartQuery(
           ICalculationService calculatePrice,
           IDataFromCookie regionFromCookie,
           ILogger<CartQuery> logger,
           IUserRepository<User> userRepository,
           IRepository<Setting> setingsRepository,
           IProductRepository<Product> productRepository,
           IRepository<CartItem> cartItemRepository)
        {
            _calculatePrice = calculatePrice;
            _regionFromCookie = regionFromCookie;
            _logger = logger;
            _userRepository = userRepository;
            _setingsRepository = setingsRepository;
            _productRepository = productRepository;
            _cartItemRepository = cartItemRepository;
        }

        public async Task<CartDto> UserCart()
        {
            try
            {
                var region = _regionFromCookie.GetUserRegion();
                var tgId = _regionFromCookie.GetUserTgID();
                _logger.LogInformation("Fetching user cart for TG ID: {TgId}", tgId);

                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found {tgId}");
                    return new CartDto();
                }

                var userCartItems = user.Cart.CartItems.ToList(); // Загружаем в память

                var cart = new List<CartItemDto>();

                foreach (var item in userCartItems)
                {
                    var price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type);
                    var jPrice = await _calculatePrice.CalcJprice(price);
                    var tmp = new CartItemDto();

                    switch (item.Product.Type)
                    {
                        case "Game":
                            var edition = await _productRepository.GetTypeEntity<Edition>(item.Product);
                            tmp.image = edition.Image;
                            tmp.Name = edition.Name;
                            tmp.EditionName = edition.EditionType;
                            tmp.Id = item.Guid;
                            tmp.ProductId = item.ProductId;
                            tmp.Discount = item.Product.DiscountPercent;
                            tmp.Price = price;
                            tmp.JPrice = jPrice;
                            tmp.Platform = edition.Platform;
                            break;
                        case "AddOn":
                            var addOn = await _productRepository.GetTypeEntity<AddOn>(item.Product);
                            tmp.image = addOn.Image;
                            tmp.Name = addOn.Name;
                            tmp.EditionName = "";
                            tmp.Id = item.Guid;
                            tmp.ProductId = item.ProductId;
                            tmp.Discount = item.Product.DiscountPercent;
                            tmp.Price = price;
                            tmp.JPrice = jPrice;
                            tmp.Platform = addOn.Platform;
                            break;
                        case "Subscription":
                            var sub = await _productRepository.GetTypeEntity<Subscription>(item.Product);
                            tmp.image = sub.Image;
                            tmp.Name = sub.Name;
                            tmp.EditionName = "";
                            tmp.Id = item.Guid;
                            tmp.ProductId = item.ProductId;
                            tmp.Discount = item.Product.DiscountPercent;
                            tmp.Price = price;
                            tmp.JPrice = jPrice;
                            tmp.Platform = sub.Platform;
                            break;
                    }

                    cart.Add(tmp);
                }

                var settings = (await _setingsRepository.GetListQuery()).FirstOrDefault(s => s.UserId == user.Guid && s.Region == region);

                var result = new CartDto
                {
                    items = cart,
                    Email = settings?.EmailPsStore ?? "",
                    PayEmail = user.Email ?? "",
                    Password = settings?.PasswordPsStore ?? "",
                    Code = settings?.Code ?? ""
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task UpdateUserCart(Guid ProductId)
        {
            try
            {
                var tgId = _regionFromCookie.GetUserTgID();
                _logger.LogInformation($"Updating user {tgId} cart: {ProductId}");

                var product = await _productRepository.GetById(ProductId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", ProductId);
                    throw new Exception($"Product with GUID {ProductId} not found");
                }

                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).FirstOrDefault(u => u.TgUserId == tgId);
                if (user is null)
                {
                    _logger.LogError("User with id {id} not found", tgId);
                    throw new Exception($"User with tg id {tgId} not found");
                }
                if(user.Cart.CartItems.FirstOrDefault( c=> c.ProductId == product.Guid) != null)
                {
                    return;
                }
                var result = new CartItem()
                {
                    CartId = user.Cart.Guid,
                    ProductId = product.Guid,
                };

                await _cartItemRepository.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task DeleteFromCart(Guid ProductId)
        {
            try
            {
                var tgId = _regionFromCookie.GetUserTgID();
                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).FirstOrDefault(u => u.TgUserId == tgId);

                if (user is null)
                {
                    _logger.LogError("User Cart with id {id} not found", tgId);
                    throw new Exception($"User Cart with id {tgId} not found");
                }

                var item = user.Cart.CartItems.FirstOrDefault(c => c.ProductId == ProductId);
                if (item is null)
                {
                    _logger.LogError("CartItem with id {id} not found in User {id} Cart", ProductId, tgId);
                    throw new Exception($"User Cart with id {tgId} not found");
                }
                await _cartItemRepository.HardDelete(item.Guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
