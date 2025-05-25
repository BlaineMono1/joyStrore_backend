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
        private readonly ICalculationService _calculatePrice;
        private readonly ILogger<SubscriptionsQuerys> _logger;
        private readonly IDataFromCookie _dataFromCookie;

        public SubscriptionsQuerys(ICalculationService calculatePrice,
            ILogger<SubscriptionsQuerys> logger,
            IProductRepository<Product> productRepository,
            ISubscriptionRepository<Subscription> subscriptionRepository,
            IDataFromCookie dataFromCookie)
        {
            _calculatePrice = calculatePrice;
            _logger = logger;

            _productRepository = productRepository;
            _subscriptionRepository = subscriptionRepository;
            _dataFromCookie = dataFromCookie;
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
            if (Price < 0) throw new BadRequestExeption("Price can't be lower then 0");

            var product = await _productRepository.GetEntityType(SubId);

            if (product == null) throw new NotFoundException(nameof(Subscription), SubId);

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
            var region = _dataFromCookie.GetUserRegion();
            var result = new List<DiscountSubDto>();

            var settings = (await _subscriptionRepository.GetListQuery()).Include(s => s.Product).ToList();

            result.AddRange(settings.Select(item => new DiscountSubDto
            {
                Id = item.Guid,
                Percent = (region == "UAH" ? item.Product.DiscountPercentUa : item.Product.DiscountPercentTr),
                SectionName = item.SectionName,
                Duration = item.Duration
            }
            ));

            return result;
        }

    //    public async Task UpdateSubDiscount(Guid SubId, string Percent)
    //    {
    //        if (decimal.Parse(Percent) < 0) throw new BadRequestExeption("Price can't be lower then 0");
    //        if (decimal.Parse(Percent) > 100) throw new BadRequestExeption("Price can't be greater then 100");

    //        var product = await _productRepository.GetEntityType(SubId);

    //        if (product == null) throw new NotFoundException(nameof(Subscription), SubId);

    //        product.DiscountPercent = Percent;

    //        if (decimal.Parse(Percent) > 0) product.DiscountDate = DateTime.MaxValue;
    //        else if (decimal.Parse(Percent) == 0) product.DiscountDate = null;

    //        await _productRepository.Update(product);
    //    }

    }
}
