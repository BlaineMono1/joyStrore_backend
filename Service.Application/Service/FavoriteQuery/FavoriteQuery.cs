using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.FavoriteQuery.Dto;


namespace Service.Application.Service.FavoriteQuery
{
    public class FavoriteQuery
    {
        private readonly IUserRepository<User> _userRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IRepository<FavoriteItem> _favoriteItemRepository;
        private readonly IRepository<Cart> _cartRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<FavoriteQuery> _logger;
        private readonly IDataFromCookie _regionFromCookie;

        public FavoriteQuery(
            ICalculationService calculatePrice,
            ILogger<FavoriteQuery> logger,
            IUserRepository<User> userRepository,
            IProductRepository<Product> productRepository,
            IRepository<FavoriteItem> favoriteItemRepository,
            IRepository<Cart> cartRepository,
            IDataFromCookie regionFromCookie)


        {
            _calculatePrice = calculatePrice;
            _logger = logger;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _favoriteItemRepository = favoriteItemRepository;
            _regionFromCookie = regionFromCookie;
            _cartRepository = cartRepository;
        }

        public async Task<List<FavoriteDto>> UserFavorite()
        {
            try
            {
                var tgId = _regionFromCookie.GetUserTgID();
                _logger.LogInformation("Fetching user favorite items for ID: {TgId}", tgId);

                var user = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {TgId}", tgId);
                    return new List<FavoriteDto>();
                }

                var favoriteItems = user.Favorite.FavoriteItems?.Where(item => !item.IsDelete).ToList() ?? Enumerable.Empty<FavoriteItem>();

                var result = new List<FavoriteDto>();
                foreach (var item in favoriteItems)
                {
                    var price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type);
                    var jPrice = await _calculatePrice.CalcJprice(price);
                    var tmp = new FavoriteDto();
                    switch (item.Product.Type)
                    {
                        case "Game":
                            var edition = await _productRepository.GetTypeEntity<Edition>(item.Product);
                            tmp.image = edition.Image;
                            tmp.Name = edition.EditionName;
                            tmp.EditionName = edition.EditionType;
                            tmp.Id = item.Guid;
                            tmp.ProductId = item.ProductId;
                            tmp.Discount = item.Product.DiscountPercent;
                            tmp.Price = price;
                            tmp.JPrice = jPrice;
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
                            break;
                    }

                    var cart = (await _cartRepository.GetListQuery()).Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user.Guid);
                    tmp.InCart = (cart.CartItems is null) ? false : cart.CartItems.Any(c => c.ProductId == item.ProductId);
                    result.Add(tmp);
                }

                _logger.LogInformation("Successfully fetched user favorite items for ID: {TgId}", tgId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task UpdateUserFavorites(Guid productId)
        {
            try
            {
                var tgId = _regionFromCookie.GetUserTgID();
                _logger.LogInformation($"Updating user {tgId} favorites: {productId}");

                var product = await _productRepository.GetById(productId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", productId);
                    throw new Exception($"Product with GUID {productId} not found");
                }

                var user = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(f => f.FavoriteItems).FirstOrDefault(u => u.TgUserId == tgId);
                if (user is null)
                {
                    _logger.LogError("User with id {id} not found", tgId);
                    throw new Exception($"User with tg id {tgId} not found");
                }

                if (user.Favorite.FavoriteItems.FirstOrDefault(f => f.ProductId == product.Guid) != null) return;

                var result = new FavoriteItem()
                {
                    ProductId = product.Guid,
                    FavoriteId = user.Favorite.Guid
                };

                await _favoriteItemRepository.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }


        public async Task DeleteFromFavorites(Guid ProductId)
        {
            try
            {
                var tgId = _regionFromCookie.GetUserTgID();
                _logger.LogInformation($"Deliting from user {tgId} favorites: {ProductId}");

                var user = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).FirstOrDefault(u => u.TgUserId == tgId);

                if (user is null)
                {
                    _logger.LogError("User with id {id} not found", tgId);
                    throw new Exception($"User with id {tgId} not found");
                }

                var item = user.Favorite.FavoriteItems.FirstOrDefault(c => c.ProductId == ProductId);
                if (item is null)
                {
                    _logger.LogError("FavoriteItem with id {id} not found in User {id} Favorites", ProductId, tgId);
                    throw new Exception($"User FavoriteItem with id {tgId} not found");
                }
                await _favoriteItemRepository.HardDelete(item.Guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
       
    }
}
