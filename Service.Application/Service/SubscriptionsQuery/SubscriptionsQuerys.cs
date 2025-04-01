using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.SubscriptionsQuery.Dto;

namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly ISubscriptionRepository<Subscription> _subscriptionRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<SubscriptionsQuerys> _logger;

        public SubscriptionsQuerys(ICalculationService calculatePrice,
            ILogger<SubscriptionsQuerys> logger,
            IProductRepository<Product> productRepository,
            ISubscriptionRepository<Subscription> subscriptionRepository)
        {
            _calculatePrice = calculatePrice;
            _logger = logger;

            _productRepository = productRepository;
            _subscriptionRepository = subscriptionRepository;

        }

        /// <summary>
        /// Выдача списка подписок
        /// </summary>
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList()
        {
            try
            {
                var subscriptions = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).ToList();

                _logger.LogInformation("Fetched {Count} subscriptions.", subscriptions.Count);

                var tasks = subscriptions.Select(async sub =>
                {
                    try
                    {
                        var product = await _productRepository.GetById(sub.ProductId)
                            ?? throw new KeyNotFoundException($"Product with TypeId {sub.Guid} not found");

                        var price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type);
                        var jPrice = await _calculatePrice.CalcJprice(price);

                        return new SubscriptionsListDto
                        {
                            ProductId = (await _productRepository.GetEntityType(sub.Guid)).Guid,
                            Name = sub.Name,
                            ImagePath = sub.Image,
                            Dicount = product.DiscountPercent,
                            Price = price,
                            Jprice = jPrice,
                            SectionName = sub.SectionName
                            
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing subscription {SubscriptionId}", sub.Guid);
                        return null;
                    }
                });

                var result = (await Task.WhenAll(tasks)).Where(t => t != null).ToList();
                _logger.LogInformation("Successfully processed {Count} subscriptions.", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscriptions list.");
                throw;
            }
        }


        public async Task<List<PriceSubDto>> GetPriceSubList()
        {
            var result = new List<PriceSubDto>();

            var settings = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).ToList();

            result.AddRange(settings.Select(item => new PriceSubDto
            {
                Id = item.Guid,
                PriceUAH = item.Product.PriceUa,
                PriceTRY = item.Product.PriceTr,
                SectionName = item.SectionName,
                Duration = item.Duration
            }
            ));

            return result;
        }

        public async Task UpdateSubPrice(Guid SubId, decimal Price, string Region)
        {
            if (Price < 0) throw new Exception("Price can't be lower then 0");

            var product = await _productRepository.GetEntityType(SubId);

            if (product == null) throw new Exception($"Product for subscription with GUID {SubId} not found");

            switch (Region)
            {
                case "UAH":
                    product.PriceUa = Price;
                    break;
                case "TRY":
                    product.PriceTr = Price;
                    break;

            }

            await _productRepository.Update(product);
        }

        public async Task<List<DiscountSubDto>> GetDiscountSubList()
        {
            var result = new List<DiscountSubDto>();

            var settings = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).ToList();

            result.AddRange(settings.Select(item => new DiscountSubDto
            {
                Id = item.Guid,
                Percent = item.Product.DiscountPercent,
                SectionName = item.SectionName,
                Duration = item.Duration
            }
            ));

            return result;
        }

        public async Task UpdateSubDiscount(Guid SubId, string Percent)
        {
            if (decimal.Parse(Percent) < 0) throw new Exception("Price can't be lower then 0");
            if (decimal.Parse(Percent) > 100) throw new Exception("Price can't be greater then 100");

            var product = await _productRepository.GetEntityType(SubId);

            if (product == null) throw new Exception($"Product for subscription with GUID {SubId} not found");

            product.DiscountPercent = Percent;

            if (decimal.Parse(Percent) > 0) product.DiscountDate = DateTime.MaxValue;
            else if (decimal.Parse(Percent) == 0) product.DiscountDate = null;

            await _productRepository.Update(product);
        }

    }
}
