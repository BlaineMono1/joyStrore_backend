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

        public FavoriteQuery(
            ICalculationService calculatePrice,
            ILogger<FavoriteQuery> logger,
            IUserRepository<User> userRepository,
            IProductRepository<Product> productRepository,
            IRepository<FavoriteItem> favoriteItemRepository)


        {
            _calculatePrice = calculatePrice;
            _logger = logger;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _favoriteItemRepository = favoriteItemRepository;
        }

        public async Task<List<FavoriteDto>> UserFavorite(Guid userId)
        {
            try
            {

                _logger.LogInformation("Fetching user favorite items for ID: {TgId}", userId);

                var user = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.Guid == userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {TgId}", userId);
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

                _logger.LogInformation("Successfully fetched user favorite items for ID: {TgId}", userId);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user favorite items for ID: {TgId}", userId);
                throw;
            }
        }

        public async Task UpdateUserFavorites(Guid UserId, Guid productId)
        {
            try
            {
                _logger.LogInformation($"Updating user {UserId} favorites: {productId}");

                var product = await _productRepository.GetEntityType(productId);
                if (product is null)
                {
                    _logger.LogError("No Product with GUID {id}", productId);
                    throw new Exception($"Product with GUID {productId} not found");
                }

                var fav = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.Guid == UserId).Favorite;
                if (fav is null)
                {
                    _logger.LogError("User Favorite with id {id} not found", UserId);
                    throw new Exception($"User Favorite with tg id {UserId} not found");
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
                _logger.LogError(ex, "Error updating user favorite Id {tgid}, itemId {itemId}", UserId, productId);
                throw;
            }
        }


        public async Task DeleteFromFavorites(Guid userId, Guid ProductId)
        {
            try
            {
                _logger.LogInformation($"Deliting from user {userId} favorites: {ProductId}");

                var fav = (await _userRepository.GetListQuery()).Include(u => u.Favorite).ThenInclude(c => c.FavoriteItems).ThenInclude(i => i.Product).FirstOrDefault(u => u.Guid == userId).Favorite.FavoriteItems;

                if (fav is null)
                {
                    _logger.LogError("User Favorites with id {id} not found", userId);
                    throw new Exception($"User Favorites with id {userId} not found");
                }

                var item = fav.FirstOrDefault(c => c.ProductId == ProductId);
                if (item is null)
                {
                    _logger.LogError("FavoriteItem with id {id} not found in User {id} Favorites", ProductId, userId);
                    throw new Exception($"User FavoriteItem with id {userId} not found");
                }
                await _favoriteItemRepository.HardDelete(item.Guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user favorite Id {tgid}, itemId {itemId}", userId, ProductId);
                throw;
            }
        }
       
    }
}
