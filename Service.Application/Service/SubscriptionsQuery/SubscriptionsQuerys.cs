using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Iterfaces;
using Service.Application.Service.SubscriptionsQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Service.Application.Service.SubscriptionsQuery
{
    public class SubscriptionsQuerys
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly ISubscriptionRepository<Subscription> _subscriptionRepository;
        private readonly IRepository<PriceSettingSubscription> _priceStiingSubRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<SubscriptionsQuerys> _logger;
        private readonly IDataFromCookie _dataFromCookie;

        public SubscriptionsQuerys(ICalculationService calculatePrice,
            ILogger<SubscriptionsQuerys> logger,
            IProductRepository<Product> productRepository,
            ISubscriptionRepository<Subscription> subscriptionRepository,
            IDataFromCookie dataFromCookie,
            IRepository<PriceSettingSubscription> priceStiingSubRepository)
        {
            _calculatePrice = calculatePrice;
            _logger = logger;

            _productRepository = productRepository;
            _subscriptionRepository = subscriptionRepository;
            _dataFromCookie = dataFromCookie;
            _priceStiingSubRepository = priceStiingSubRepository;
        }

        /// <summary>
        /// Выдача списка подписок
        /// </summary>
        public async Task<List<SubscriptionsListDto>> GetSubscriptionsList()
        {

            var region = _dataFromCookie.GetUserRegion();
            var subscriptions = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).ToList();

            _logger.LogInformation("Fetched {Count} subscriptions.", subscriptions.Count);

            var result = new List<SubscriptionsListDto>();

            foreach (var sub in subscriptions)
            {
                var product = await _productRepository.GetById(sub.ProductId)
                                ?? throw new NotFoundException(nameof(Product), sub.ProductId);

                var price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type, product.Guid);
                var jPrice = await _calculatePrice.CalcJprice(price);

                result.Add(new SubscriptionsListDto
                {
                    ProductId = product.Guid,
                    Name = sub.Name,
                    ImagePath = sub.Image,
                    Dicount = (region == "UAH" ? product.DiscountPercentUa : product.DiscountPercentTr),
                    Price = price,
                    Jprice = jPrice,
                    SectionName = sub.SectionName
                });
            }

            return result;

        }

        public async Task<List<MarkUpSubDto>> GetMarkUpList()
        {
            var result = new List<MarkUpSubDto>();

            var region = _dataFromCookie.GetUserRegion();


            var markUp = (await _priceStiingSubRepository.GetListQuery()).Include(m => m.Subscription).Where(m => m.Region == region).ToList();

            result.AddRange(markUp.Select(item => new MarkUpSubDto
            {
                Id = item.Guid,
                Name = item.Subscription.Name,
                Percent = item.Percent
            }
            ));

            return result;
        }

        public async Task UpdatePercent(Guid Id, decimal Percent)
        {
            if (Percent < 0) throw new BadRequestExeption("Invalid Percent value");

            var markUp = await _priceStiingSubRepository.GetById(Id) 
                ?? throw new NotFoundException(nameof(PriceSettingSubscription), $"Subscription mark up with Guid not found");

            markUp.Percent = Percent;

            await _priceStiingSubRepository.Update(markUp);

        }
        

    }
}
