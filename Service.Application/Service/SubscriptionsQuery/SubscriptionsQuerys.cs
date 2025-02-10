using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;
using Service.Application.Service.SubscriptionsQuery.Dto;

namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly ProductRepository<Product> _productRepository;
        private readonly SubscriptionRepository<Subscription> _subscriptionRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        public SubscriptionsQuerys(ICalculationService calculatePrice, IHttpContextAccessor httpContextAccessor)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
        }
       
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList() // Выдача списка подписок
        {
            string region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            var subscriptions = await _subscriptionRepository.GetAllList(); // все подписки 

            var result = new List<SubscriptionsListDto>();

            foreach (var sub in subscriptions)
            {
                var t = new SubscriptionsListDto();
                var product = await _productRepository.GetById(sub.ProductId) ?? throw new KeyNotFoundException($"Product with TypeId {sub.Guid} not found");
                // Продукт соответствующий подписке
                t.Name = sub.Name;
                t.ImagePath = sub.Image;
                if (product.DiscountDate >= DateTime.UtcNow) // Если скидка есть
                {
                    t.Dicount = product.DiscountPercent;
                    decimal? price = region switch // Как регион хранится в куки??
                    {
                        "UA" => product.DiscountUa,
                        "TR" => product.DiscountTr,
                        _ => throw new Exception("No region")

                    };
                    t.Price = await _calculatePrice.CalcPrice(price, product.Type, region);
                    t.Jprice = await _calculatePrice.CalcJprice(t.Price, region);
                }
                else
                {
                    t.Dicount = "0";
                    decimal? price = region switch // Как регион хранится в куки??
                    {
                        "UA" => product.PriceUa,
                        "TR" => product.PriceTr,
                        _ => throw new Exception("No region")

                    };
                    t.Price = await _calculatePrice.CalcPrice(price, product.Type, region);
                    t.Jprice = await _calculatePrice.CalcJprice(t.Price, region);
                }
                result.Add(t);
                
            }

            return result;
        }

        public async Task<SubscriptionDto> SubscriptionById(Guid Id)
        {
            var result = new SubscriptionDto();

            string region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            var currentSub = await _subscriptionRepository.GetById(Id);

            var subs = await _subscriptionRepository.SubscriptionsByName(currentSub.Name);

            var prod = await _productRepository.GetEntityType(Id);


            result.Id = Id;
            result.Image = currentSub.Image;
            result.Type = prod.Type;
            result.Platform = currentSub.Platform;
            result.Subscriptions = subs;

            if (prod.DiscountDate >= DateTime.UtcNow) // Если скидка есть
            {
                result.Discount = prod.DiscountPercent;
                decimal? price = region switch // Как регион хранится в куки??
                {
                    "UA" => prod.DiscountUa,
                    "TR" => prod.DiscountTr,
                    _ => throw new Exception("No region")

                };
                result.Price = await _calculatePrice.CalcPrice(price, prod.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
            }
            else
            {
                result.Discount = "0";
                decimal? price = region switch // Как регион хранится в куки??
                {
                    "UA" => prod.PriceUa,
                    "TR" => prod.PriceTr,
                    _ => throw new Exception("No region")

                };
                result.Price = await _calculatePrice.CalcPrice(price, prod.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
            }

            return result;
        }
    }
}
