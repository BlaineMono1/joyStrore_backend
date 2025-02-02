using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;
using Service.Application.Service.SubscriptionsQuery.Dto;

namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly Repository<Product> _productRepository;
        private readonly Repository<Subscription> _subscriptionRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SubscriptionsQuerys(ICalculationService calculatePrice, IHttpContextAccessor httpContextAccessor)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetUserRegion()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request.Cookies.TryGetValue("region", out string region) == true)
            {
                return region;
            }

            return "default"; // Значение по умолчанию, если кука не найдена
        }
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList() // Выдача списка подписок
        {
            string region = GetUserRegion();

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
    }
}
