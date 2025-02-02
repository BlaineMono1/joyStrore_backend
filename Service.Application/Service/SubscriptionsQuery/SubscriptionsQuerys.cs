using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using Service.Application.Service.CalculationService;
using Service.Application.Service.GetNewsList.Dto;
using Service.Application.Service.SubscriptionsQuery.Dto;
namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly Repository<Product> _productRepository;
        private readonly Repository<Subscription> _subscriptionRepository;
        private readonly CalculatePrice _calculatePrice;
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList()
        {
            var products = await _productRepository.GetAllList();
            var subscriptions = await _subscriptionRepository.GetAllList();

            var result = new List<SubscriptionsListDto>();

            foreach (var sub in subscriptions)
            {
                var t = new SubscriptionsListDto();
                var product = products.FirstOrDefault(p => p.TypeId == sub.Guid) ?? throw new KeyNotFoundException($"Product with TypeId {sub.Guid} not found");
                t.Name = sub.Name;
                t.ImagePath = sub.Image;
                t.PriceUa = await _calculatePrice.CalcPrice(product.PriceUa, product.Type, "UA");
                t.PriceTr = await _calculatePrice.CalcPrice(product.PriceUa, product.Type, "TR");
                t.JpriceUa = await _calculatePrice.CalcJprice(t.PriceUa, "UA");
                t.JpriceTr = await _calculatePrice.CalcJprice(t.PriceTr, "TR"); ;
            }

            return result;
        }
    }
}
