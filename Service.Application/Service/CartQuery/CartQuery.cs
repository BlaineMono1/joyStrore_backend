using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.CartQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

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

            var region = _regionFromCookie.GetUserRegion();
            var tgId = _regionFromCookie.GetUserTgID();
            _logger.LogInformation("Fetching user cart for TG ID: {TgId}", tgId);

            var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), tgId);
            }
            var cart = new List<CartItemDto>();

            var userCartItems = user.Cart.CartItems.ToList(); // Загружаем в память

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
                        tmp.Discount = (region == "UAH" ? item.Product.DiscountPercentUa : item.Product.DiscountPercentTr);
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
                        tmp.Discount = (region == "UAH" ? item.Product.DiscountPercentUa : item.Product.DiscountPercentTr);
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
                        tmp.Discount = (region == "UAH" ? item.Product.DiscountPercentUa : item.Product.DiscountPercentTr);
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
                Code = settings?.Code ?? "",
                CartSize = cart.Count
            };

            return result;

        }

        public async Task UpdateUserCart(Guid ProductId)
        {

            var tgId = _regionFromCookie.GetUserTgID();
            _logger.LogInformation($"Updating user {tgId} cart: {ProductId}");

            var product = await _productRepository.GetById(ProductId);
            if (product is null)
            {
                throw new NotFoundException(nameof(Product), ProductId);
            }

            var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).FirstOrDefault(u => u.TgUserId == tgId);
            if (user is null)
            {
                throw new NotFoundException(nameof(User), tgId);
            }

            if (user.Cart.CartItems.FirstOrDefault(c => c.ProductId == product.Guid) != null)
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

        public async Task DeleteFromCart(Guid ProductId)
        {

            var tgId = _regionFromCookie.GetUserTgID();
            var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).FirstOrDefault(u => u.TgUserId == tgId);

            if (user is null)
            {
                throw new NotFoundException(nameof(User), tgId);
            }

            var item = user.Cart.CartItems.FirstOrDefault(c => c.ProductId == ProductId);
            if (item is null)
            {
                throw new NotFoundException(nameof(CartItem), ProductId);
            }
            await _cartItemRepository.HardDelete(item.Guid);

        }
    }
}
