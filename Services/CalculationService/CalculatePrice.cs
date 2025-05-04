using Business.Data.Models;
using Service.Application.Iterfaces;
using Microsoft.Extensions.Logging;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Services.CalculationService.Dto;


namespace Services.CalculationService
{
    public class CalculatePrice : ICalculationService
    {
        private readonly ICacheService _cacheService;
        private readonly IRepository<SettingPrice> _settingPriceRepository;
        private readonly IRepository<PriceSettingSubscription> _priceSettingSubscription;
        private readonly IRepository<LoyaltySetting> _loyaltySettingRepository;
        private readonly IRedisRepository _redis; // redis
        private readonly ILogger<CalculatePrice> _logger;
        
        private readonly IDataFromCookie _regionFromCookie;

        public CalculatePrice(
            IRepository<SettingPrice> settingPriceRepository,
            IRepository<PriceSettingSubscription> priceSettingSubscription,
            IRepository<LoyaltySetting> loyaltySettingRepository,
            IRedisRepository redis,
            ILogger<CalculatePrice> logger,
            IHttpContextAccessor httpContextAccessor,
            IDataFromCookie regionFromCookie,
            ICacheService cacheService)
        {
            _settingPriceRepository = settingPriceRepository;
            _priceSettingSubscription = priceSettingSubscription;
            _loyaltySettingRepository = loyaltySettingRepository;
            _redis = redis;
            _logger = logger;
           
            _regionFromCookie = regionFromCookie;
            _cacheService = cacheService;
        }

        private async Task<decimal> GetPrice(string region, decimal? price)
        {
            if (price == null)
            {
                _logger.LogWarning("Price is null for region {Region}. Returning 0.", region);
                return 0;
            }
            _logger.LogInformation($"Calculating price for {region} - {price.Value}");
            decimal exchangeRate = 0;
           
            if (region == "UAH")
            {
                string? cachedData = await _redis.GetAsync("UAH");
                _logger.LogInformation($"Data from redis - {cachedData}");
                if(cachedData is null)
                {
                    await UpdateCahce();
                    cachedData = await _redis.GetAsync("UAH");
                    _logger.LogInformation($"Data from redis after update - {cachedData}");
                }
                if (float.TryParse(cachedData, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedDecimal))
                {
                    exchangeRate = (decimal)parsedDecimal;
                    _logger.LogInformation($"Fetched data drom redis {region} - {exchangeRate}");
                }
                else
                {
                    _logger.LogError($"Can not parse data from redis {cachedData}"); 
                }

            }
            else if(region == "TRY")
            {
                string? cachedData = await _redis.GetAsync("TRY");
                _logger.LogInformation($"Data from redis - {cachedData}");
                if (cachedData is null)
                {
                    await UpdateCahce();
                    cachedData = await _redis.GetAsync("TRY");
                    _logger.LogInformation($"Data from redis after update - {cachedData}");

                }
                if (float.TryParse(cachedData, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedDecimal))
                {
                    exchangeRate = (decimal)parsedDecimal;
                    _logger.LogInformation($"Fetched data drom redis {region} - {exchangeRate}");
                }
                else
                {
                    _logger.LogError($"Can not parse data from redis {cachedData}");
                }
            }
            else
            {
                _logger.LogError($"No region {region}");
            }

            return price.Value * 2;
        }

        public async Task<decimal> CalcPrice(decimal? priceua, decimal? pricetr, string type)
        {
            var region = _regionFromCookie.GetUserRegion();
            try
            {                
                _logger.LogInformation("Calculating price for region {Region}, type {Type}.", region, type);

                var price = region switch
                {
                    "UAH" => priceua,
                    "TRY" => pricetr,
                    _ => throw new Exception("No region found")
                };

                if (price == null)
                {
                    _logger.LogError("Price is null for region {Region}. Returning 0.", region);
                    return 0;
                }

                var rubPrice = await GetPrice(region, price);

                decimal priceWithMarkup = 0;
                // Fetch markup data from the repository
                //var markupGame = (await _settingPriceRepository.GetListQuery()).OrderByDescending(p => p.Price).FirstOrDefault(p => rubPrice >= p.Price);
                //var markupSub = (await _priceSettingSubscription.GetListQuery()).FirstOrDefault(s => s.Region == region);
                var prices = new PricesDto();
                decimal p = 0M;
                foreach (var t in prices.l) 
                {
                    if(t > price)
                    {
                        break;
                    }
                    p = t;
                }
                string? cachedData = await _redis.GetAsync($"MarkUpGame-{p}");
                if(cachedData is null)
                {
                    await _cacheService.UpdateMarkUp();
                    cachedData = await _redis.GetAsync($"MarkUpGame-{p}");
                }
                decimal? markupGame = null;
                markupGame = decimal.Parse(cachedData);
                if (markupGame == null) //|| markupSub == null)
                {
                    _logger.LogError("Markup data not found for the given price: {Price}.", rubPrice);
                    throw new Exception("Markup data not found.");
                }

                switch (type)
                {
                    case "Game":
                        priceWithMarkup = rubPrice * markupGame.Value + rubPrice;
                        break;
                    case "AddOn":
                        priceWithMarkup = rubPrice * markupGame.Value + rubPrice;
                        break;

                    case "Subscription":
                        priceWithMarkup = 0;//rubPrice * markupSub.Percent + rubPrice;
                        break;

                    default:
                        _logger.LogWarning("Unknown type: {Type}. Returning 0.", type);
                        priceWithMarkup = 0;
                        break;
                }

                _logger.LogInformation("Calculated price with markup: {PriceWithMarkup}", priceWithMarkup);
                return Math.Round(priceWithMarkup, MidpointRounding.AwayFromZero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for region {Region}, type {Type}.", region, type);
                throw;
            }
        }

        public async Task<decimal> CalcJprice(decimal? price)
        {
            var region = _regionFromCookie.GetUserRegion();
            try
            {

                if (price == null)
                {
                    _logger.LogWarning("Price is null for region {Region}. Returning 0.", region);
                    return 0;
                }

                _logger.LogInformation("Calculating JPrice for price {Price} and region {Region}.", price, region);

                var loyality = (await _loyaltySettingRepository.GetListQuery()).FirstOrDefault(l => l.PriceValue >= price.Value);
               
                if (loyality == null)
                {
                    _logger.LogError("No loyalty data found for price {Price}.", price);
                    throw new KeyNotFoundException("Loyalty setting not found");
                }

                decimal jPrice = price.Value - price.Value * loyality.DiscountPercent;
                _logger.LogInformation("Calculated JPrice: {JPrice}", jPrice);
                return jPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating JPrice for price {Price} and region {Region}.", price, region);
                throw;
            }
        }

        public async Task<decimal> CalcJplus(decimal price)
        {
            try
            {
                _logger.LogInformation("Calculating JPlus for price {Price}.", price);

                var cachedData = await _redis.GetAsync("cashback");
                _logger.LogInformation($"Data from redis for cashback - {cachedData}");
                decimal cashback = 0;

                if(cachedData is null)
                {
                   await _cacheService.UpdateCashBack();
                   cachedData = await _redis.GetAsync("cashback");
                    _logger.LogInformation($"Data from redis for cashback - {cachedData}");
                }

                if (decimal.TryParse(cachedData, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedDecimal))
                {
                    cashback = parsedDecimal;
                    _logger.LogInformation($"Fetched data for cashback - {cashback}");
                }

                decimal jPlus = Math.Round(price * cashback / 100, MidpointRounding.AwayFromZero);
                _logger.LogInformation("Calculated JPlus: {JPlus}", jPlus);
                return jPlus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating JPlus for price {Price}.", price);
                throw;
            }
        }

        private async Task UpdateCahce()
        {
            await _cacheService.UpdateExchangeRates();
        }
    }
}
