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
        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<FavoriteQuery> _logger;
        private readonly IDataFromCookie _regionFromCookie;

        public FavoriteQuery(
            ICalculationService calculatePrice,
            ILogger<FavoriteQuery> logger,
            IUserRepository<User> userRepository,
            IProductRepository<Product> productRepository,
            IRepository<FavoriteItem> favoriteItemRepository,
            IDataFromCookie regionFromCookie)


        {
            _calculatePrice = calculatePrice;
            _logger = logger;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _favoriteItemRepository = favoriteItemRepository;
            _regionFromCookie = regionFromCookie;
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

                var favoriteItems = user.Favorite.FavoriteItems?.Where(item => !item.IsDelete) ?? Enumerable.Empty<FavoriteItem>();

                var result = await Task.WhenAll(favoriteItems.Select(async item =>
                {
                    var price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type);
                    var jPrice = await _calculatePrice.CalcJprice(price);
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

                _logger.LogInformation("Successfully fetched user favorite items for ID: {TgId}", tgId);
                return result.ToList();
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

                var product = await _productRepository.GetEntityType(productId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", productId);
                    throw new Exception($"Product with GUID {productId} not found");
                }

                var fav = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId).Favorite;
                if (fav is null)
                {
                    _logger.LogError("User Favorite with id {id} not found", tgId);
                    throw new Exception($"User Favorite with tg id {tgId} not found");
                }
                var result = new FavoriteItem()
                {
                    ProductId = product.Guid,
                    FavoriteId = fav.Guid
                };



                fav.FavoriteItems ??= new List<FavoriteItem>();
                fav.FavoriteItems.Add(result);
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

                var fav = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.TgUserId == tgId).Favorite.FavoriteItems;

                if (fav is null)
                {
                    _logger.LogError("User Favorites with id {id} not found", tgId);
                    throw new Exception($"User Favorites with id {tgId} not found");
                }

                var item = fav.FirstOrDefault(c => c.ProductId == ProductId);
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
