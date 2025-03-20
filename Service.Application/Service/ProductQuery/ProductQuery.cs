using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.ProductQuery.Dto;
using System.Collections.Generic;

namespace Service.Application.Service.ProductQuery
{
    public class ProductQuery
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly IEditionRepository<Edition> _editonRepository;
        private readonly IRepository<AddOn> _addOnRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IUserRepository<User> _userRepository;


        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataFromCookie _regionFromCookie;
        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<ProductQuery> _logger;

        public ProductQuery(IProductRepository<Product> productRepository,
            IEditionRepository<Edition> editonRepository,
            IRepository<AddOn> addOnRepository,
            IRepository<Subscription> subscriptionRepository,
            IUserRepository<User> userRepository,
            IHttpContextAccessor httpContextAccessor,
            IDataFromCookie regionFromCookie,
            ICalculationService calculatePrice,
            ILogger<ProductQuery> logger)
        {
            _productRepository = productRepository;
            _editonRepository = editonRepository;
            _addOnRepository = addOnRepository;
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;

            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _calculatePrice = calculatePrice;
            _logger = logger;
        }
        public async Task<ProductDto> GetProduct(Guid ProductId)
        {
            var result = new ProductDto();
            try
            {
                var product = await _productRepository.GetById(ProductId);
                result.ProductId = ProductId;
                result.ProductType = product.Type;
                result.Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price);
                result.JPlus = await _calculatePrice.CalcJplus(result.JPrice);
                result.Discount = product.DiscountDate;
                result.DiscountPercent = product.DiscountPercent;
                var userTg = _regionFromCookie.GetUserTgID();
                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).Include(u => u.Favorite).ThenInclude(f => f.FavoriteItems)
                    .FirstOrDefault(u => u.TgUserId == userTg);

                result.InCart = user.Cart.CartItems.Any(c => c.ProductId == product.Guid);
                result.InFavorite = user.Favorite.FavoriteItems.Any(c => c.ProductId == product.Guid);
                

                switch (product.Type)
                {
                    case "Game":

                        var edition = (await _editonRepository.GetListQuery()).Include(e => e.Game)
                            .Include(e => e.EditionGeners).ThenInclude(eg => eg.Geners).FirstOrDefault(e => e.Guid == product.TypeId);
                        result.Image = edition.Image;
                        result.Geners = edition.EditionGeners.Select(eg => eg.Geners.Name).ToList();
                        result.RealiseDate = edition.Release;
                        result.Platforms = edition.Platform;
                        result.Languages = edition.Game.Languages;
                        result.Subscription = edition.Subscription;
                        result.Features = edition.Features;
                        break;

                    case "AddOn":

                        var addOn = await _addOnRepository.GetById(product.TypeId);
                        result.Image = addOn.Image;
                        result.Platforms = addOn.Platform;
                        break;

                    case "Subscription":

                        var sub = await _subscriptionRepository.GetById(product.TypeId);
                        result.Image = sub.Image;
                        result.Platforms = sub.Platform;
                        break;
                }
                result.IsPlatform = result.Platforms.Contains(user.Platform);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<List<DropDownListDto>> DropDownList(Guid ProductId)
        {
            var result = new List<DropDownListDto>();
            try
            {
                var product = await _productRepository.GetById(ProductId);
                switch (product.Type)
                {
                    case "Game":

                        var edition = (await _editonRepository.GetListQuery()).Include(e => e.Game).ThenInclude(g => g.Editions).FirstOrDefault(e => e.Guid == product.TypeId);

                        result.AddRange((IEnumerable<DropDownListDto>)Task.WhenAll(
                            edition.Game.Editions.Where(e => e.Guid != product.TypeId).Select(
                                async item =>
                                new DropDownListDto
                                {
                                    Name = item.EditionName,
                                    ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid
                                })));

                        break;
                    case "AddOn":

                        var addOn = (await _addOnRepository.GetListQuery()).Include(a => a.Game).ThenInclude(g => g.AddOns).FirstOrDefault(a => a.Guid == product.TypeId);

                        result.AddRange((IEnumerable<DropDownListDto>)Task.WhenAll(
                            addOn.Game.AddOns.Where(a => a.Guid != product.TypeId).Select(
                                async item =>
                                new DropDownListDto
                                {
                                    Name = item.Name,
                                    ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid
                                })));

                        break;

                    case "Subscription":

                        var sub = (await _subscriptionRepository.GetListQuery()).FirstOrDefault(s => s.Guid == product.TypeId); //текущая подписка
                        var groupSubs = (await _subscriptionRepository.GetListQuery()).Where(s => s.Name == sub.Name && s.Guid != sub.Guid).ToList();

                        result.AddRange((IEnumerable<DropDownListDto>)Task.WhenAll(
                            groupSubs.Select(async item =>
                                 new DropDownListDto
                                 { 
                                     Name = item.Duration,
                                     ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid
                                 }

                            )));

                        break;

                }



                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }


        public async Task<List<ProductListDto>> GetProductList(IEnumerable<Product> source)
        {
            var result = new List<ProductListDto>();
            try
            {
                foreach (var item in source)
                {
                    var t = new ProductListDto();
                    t.ProductId = item.Guid;
                    t.ImageFilepath = item.Type == "Game" ? (await _editonRepository.GetById(item.TypeId)).Image : (await _addOnRepository.GetById(item.TypeId)).Image;
                    t.Name = item.Type == "Game" ? (await _editonRepository.GetById(item.TypeId)).EditionName: (await _addOnRepository.GetById(item.TypeId)).Name;
                    t.Price = await _calculatePrice.CalcPrice(item.PriceUa, item.PriceTr, item.Type);
                    t.Jprice = await _calculatePrice.CalcJprice(t.Price);
                    t.Discount = item.DiscountPercent;
                    result.Add(t);

                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

        }
    }
}