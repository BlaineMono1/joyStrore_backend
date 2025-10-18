using System.Text.Json;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Service.MarkUpQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Service.Application.Service.MarkUpQuery
{
    public class MarkUpQUery
    {
        private readonly IRepository<SettingPrice> _setingPriceRepository;
        private readonly IRepository<PriceSettingSubscription> _subSettingPrice;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IRepository<Product> _productRepository;

        private readonly IRedisRepository _redis;
        private readonly ILogger<MarkUpQUery> _logger;

        public MarkUpQUery(
            IRepository<SettingPrice> setingPriceRepository,
            IRepository<PriceSettingSubscription> subSettingPrice,
            ILogger<MarkUpQUery> logger,
            IRedisRepository redis,
            IRepository<Subscription> subscriptionRepository,
            IRepository<Product> productRepository
        )
        {
            _setingPriceRepository = setingPriceRepository;
            _subSettingPrice = subSettingPrice;
            _redis = redis;
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
            _productRepository = productRepository;
        }

        public async Task<List<PercentDto>> GetMarkUpsProductList()
        {
            var result = new List<PercentDto>();

            var settings = (await _setingPriceRepository.GetListQuery())
                .Where(s => s.Price >= 0)
                .OrderByDescending(s => s.Region)
                .ThenBy(s => s.Price)
                .ToList();

            result.AddRange(
                settings.Select(item => new PercentDto
                {
                    Id = item.Guid,
                    Price = item.Price,
                    Percent = item.Percent,
                })
            );

            return result;
        }

        public async Task UpdatePercetProduct(Guid MarkUpId, decimal Percent)
        {
            var current = await _setingPriceRepository.GetById(MarkUpId);

            if (current is null)
            {
                throw new NotFoundException(nameof(SettingPrice), MarkUpId);
            }

            if (Percent < 0)
            {
                throw new BadRequestExeption("Percent can't be lower then 0");
            }

            current.Percent = Percent;
            var jsonData = JsonSerializer.Serialize(current.Percent);

            if (current.Region == "UAH")
            {
                var key = $"MarkUpGame-{current.Price}";

                await _redis.SetAsync(key, jsonData, null);

                await _setingPriceRepository.Update(current);
            }
            else if (current.Region == "TRY")
            {
                var key = $"MarkUpGameTR-{current.Price}";

                await _redis.SetAsync(key, jsonData, null);

                await _setingPriceRepository.Update(current);
            }
            await UpdatePercetProductAllFromDb();
        }

        public async Task UpdatePercetProductAllFromDb()
        {
            var current = await _setingPriceRepository.GetAllList();
            if (current is null)
            {
                throw new NotFoundException(nameof(SettingPrice));
            }

            foreach (var item in current.Where(c => c.Region == "UAH"))
            {
                var jsonData = JsonSerializer.Serialize(item.Percent);
                var key = $"MarkUpGame-{item.Price}";
                await _redis.SetAsync(key, jsonData, null);
                _logger.LogInformation(
                    $"Наценка успешно обновлена >{item.Price} - {item.Percent} - {item.Region}"
                );
            }
            foreach (var item in current.Where(c => c.Region == "TRY"))
            {
                var jsonData = JsonSerializer.Serialize(item.Percent);
                var keyTr = $"MarkUpGameTR-{item.Price}";
                await _redis.SetAsync(keyTr, jsonData, null);
                _logger.LogInformation(
                    $"Наценка успешно обновлена >{item.Price} - {item.Percent} - {item.Region}"
                );
            }
        }

        public async Task<List<PercentSubDto>> GetMarkUpsSubList()
        {
            var result = new List<PercentSubDto>();

            var settings = (await _subSettingPrice.GetListQuery())
                .Include(s => s.Subscription)
                .ToList();

            result.AddRange(
                settings.Select(item => new PercentSubDto
                {
                    Id = item.Guid,
                    Percent = item.Percent,
                    SectionName = item.Subscription.SectionName,
                    Duration = item.Subscription.Duration,
                })
            );

            return result;
        }

        public async Task UpdatePercentSub(Guid MarkUpId, decimal Percent)
        {
            var current = await _subSettingPrice.GetById(MarkUpId);

            if (current is null)
            {
                throw new NotFoundException(nameof(PriceSettingSubscription), MarkUpId);
            }

            if (Percent < 0)
            {
                throw new BadRequestExeption("Percent can't be lower then 0");
            }

            current.Percent = Percent;

            await _subSettingPrice.Update(current);
        }

        public async Task UpdatePriceSub(Guid subscriptionGuid, decimal priceUa, decimal pricTr)
        {
            var productSub = (await _productRepository.GetListQuery()).FirstOrDefault(p =>
                p.TypeId == subscriptionGuid
            );
            if (productSub == null)
            {
                _logger.LogInformation("Данная подписка не найдена");
                throw new NotFoundException("Данная подписка не найдена");
            }

            productSub.PriceRubUa = priceUa;
            productSub.PriceRubTr = pricTr;

            await _productRepository.Update(productSub);
        }

        public async Task<List<SubPriceList>> GetPriceSub()
        {
            List<SubPriceList> result = new List<SubPriceList>();

            var subscriptions = await (await _subscriptionRepository.GetListQuery())
                .Include(s => s.Product)
                .ToListAsync();
            if (subscriptions == null)
                throw new NotFoundException("Подписки не найдены");

            var grouped = subscriptions
                .Where(s => !string.IsNullOrEmpty(s.Duration))
                .GroupBy(s => s.Duration)
                .Select(g => new SubPriceList
                {
                    Duration = g.Key,
                    Items = g.Select(s => new SubscriptionItem
                        {
                            Id = s.Guid,
                            Name = s.Name ?? "Без названия",
                            PriceUa = s.Product.PriceRubUa,
                            PriceTr = s.Product.PriceRubTr,
                        })
                        .ToList(),
                })
                .OrderBy(x => x.Duration) // или по логике сортировки (например, по числу месяцев)
                .ToList();

            return grouped;
        }
    }
}
