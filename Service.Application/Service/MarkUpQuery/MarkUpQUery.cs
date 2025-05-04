using Business.Data.Iterfaces;
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

        private readonly ILogger<MarkUpQUery> _logger;

        public MarkUpQUery(IRepository<SettingPrice> setingPriceRepository, IRepository<PriceSettingSubscription> subSettingPrice, ILogger<MarkUpQUery> logger)
        {
            _setingPriceRepository = setingPriceRepository;
            _subSettingPrice = subSettingPrice;
            _logger = logger;
        }

        public async Task<List<PercentDto>> GetMarkUpsProductList()
        {
            var result = new List<PercentDto>();

            var settings = (await _setingPriceRepository.GetListQuery()).Where(s => s.Price > 0).ToList();

            result.AddRange(settings.Select(item => new PercentDto
            {
                Id = item.Guid,
                Percent = item.Percent
            }
            ));

            return result;
        }

        public async Task UpdatePercetProduct(Guid MarkUpId, decimal Percent)
        {
            var current = await _setingPriceRepository.GetById(MarkUpId);

            if(current is null)
            {
                throw new NotFoundException(nameof(SettingPrice), MarkUpId);
            }

            if(Percent < 0)
            {
                throw new BadRequestExeption("Percent can't be lower then 0");
            }
            if(Percent > 100)
            {
                throw new BadRequestExeption("Percent can't be greater then 100");
            }

            current.Percent = Percent;

            await _setingPriceRepository.Update(current);
        }

        public async Task<List<PercentSubDto>> GetMarkUpsSubList()
        {
            var result = new List<PercentSubDto>();

            var settings = (await _subSettingPrice.GetListQuery()).Include(s => s.Subscription).ToList();

            result.AddRange(settings.Select(item => new PercentSubDto
            {
                Id = item.Guid,
                Percent = item.Percent,
                SectionName = item.Subscription.SectionName,
                Duration = item.Subscription.Duration
            }
            ));           

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
            if (Percent > 100)
            {
                throw new BadRequestExeption("Percent can't be greater then 100");
            }

            current.Percent = Percent;

            await _subSettingPrice.Update(current);
        }
       
    }
}
