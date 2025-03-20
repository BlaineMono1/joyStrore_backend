using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
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
           IRepository<Setting> setingsRepository)
        {
            _calculatePrice = calculatePrice;
            _regionFromCookie = regionFromCookie;
            _logger = logger;
            _userRepository = userRepository;
            _setingsRepository = setingsRepository;
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

                var userCartItems = user.Cart.CartItems?.Where(item => !item.IsDelete) ?? Enumerable.Empty<CartItem>();

                var cart = await Task.WhenAll(userCartItems.Select(async item =>
                {
                    var price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type);
                    var jPrice = await _calculatePrice.CalcJprice(price);
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

                var settings = (await _setingsRepository.GetListQuery()).FirstOrDefault(s => s.UserId == user.Guid && s.Region == region);

                var result = new CartDto
                {
                    items = cart.ToList(),
                    Email = settings?.EmailPsStore ?? "",
                    PayEmail = settings?.Email ?? "",
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

                var product = await _productRepository.GetEntityType(ProductId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", ProductId);
                    throw new Exception($"Product with GUID {ProductId} not found");
                }

                var cart = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId).Cart;
                if (cart is null)
                {
                    _logger.LogError("User Cart with id {id} not found", tgId);
                    throw new Exception($"User Cart with tg id {tgId} not found");
                }
                var result = new CartItem()
                {
                    CartId = cart.Guid,
                    ProductId = product.Guid,

                };

                cart.CartItems ??= new List<CartItem>();
                cart.CartItems.Add(result);
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
                var cart = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId).Cart.CartItems;

                if (cart is null)
                {
                    _logger.LogError("User Cart with id {id} not found", tgId);
                    throw new Exception($"User Cart with id {tgId} not found");
                }

                var item = cart.FirstOrDefault(c => c.ProductId == ProductId);
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
