using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using Service.Application.Iterfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Business.Data.Iterfaces;

namespace Services.CalculationService
{
    public class CalculatePrice : ICalculationService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private readonly IRepository<SettingPrice> _settingPriceRepository;
        private readonly IRepository<PriceSettingSubscription> _priceSettingSubscription;
        private readonly IRepository<LoyaltySetting> _loyaltySettingRepository;
        private readonly IRepository<LoyaltyCashback> _cahsbackRepository; // redis
        private readonly ILogger<CalculatePrice> _logger;

        public CalculatePrice(
            IRepository<SettingPrice> settingPriceRepository,
            IRepository<PriceSettingSubscription> priceSettingSubscription,
            IRepository<LoyaltySetting> loyaltySettingRepository,
            IRepository<LoyaltyCashback> cahsbackRepository,
            ILogger<CalculatePrice> logger)
        {
            _settingPriceRepository = settingPriceRepository;
            _priceSettingSubscription = priceSettingSubscription;
            _loyaltySettingRepository = loyaltySettingRepository;
            _cahsbackRepository = cahsbackRepository;
            _logger = logger;
        }

        private async Task<decimal> GetExchangeRateUA()
        {
            _logger.LogInformation("Fetching exchange rate for UAH to RUB.");
            string url = "https://min-api.cryptocompare.com/data/price?fsym=UAH&tsyms=RUB";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonResponse);

            if (data != null && data.ContainsKey("RUB"))
            {
                decimal exchangeRate = (decimal)Math.Round(data["RUB"], MidpointRounding.AwayFromZero);
                _logger.LogInformation("Fetched exchange rate for UAH: {ExchangeRate}", exchangeRate);
                return exchangeRate;
            }
            else
            {
                _logger.LogError("Invalid response from API for UAH to RUB.");
                throw new Exception("Invalid response from API");
            }
        }

        private async Task<decimal> GetExchangeRateTR()
        {
            _logger.LogInformation("Fetching exchange rate for TRY to RUB.");
            string url = "https://min-api.cryptocompare.com/data/price?fsym=TRY&tsyms=RUB";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonResponse);

            if (data != null && data.ContainsKey("RUB"))
            {
                decimal exchangeRate = (decimal)Math.Round(data["RUB"], MidpointRounding.AwayFromZero);
                _logger.LogInformation("Fetched exchange rate for TRY: {ExchangeRate}", exchangeRate);
                return exchangeRate;
            }
            else
            {
                _logger.LogError("Invalid response from API for TRY to RUB.");
                throw new Exception("Invalid response from API");
            }
        }

        private async Task<decimal> GetPrice(string region, decimal? price)
        {
            if (price == null)
            {
                _logger.LogWarning("Price is null for region {Region}. Returning 0.", region);
                return 0;
            }

            decimal exchangeRate = region switch
            {
                "TR" => await GetExchangeRateTR(),
                "UA" => await GetExchangeRateUA(),
                _ => throw new Exception("No region found")
            };

            return price.Value * exchangeRate;
        }

        public async Task<decimal> CalcPrice(decimal? priceua, decimal? pricetr, string type, string region)
        {
            try
            {
                _logger.LogInformation("Calculating price for region {Region}, type {Type}.", region, type);

                var price = region switch
                {
                    "UA" => priceua,
                    "TR" => pricetr,
                    _ => throw new Exception("No region found")
                };

                if (price == null)
                {
                    _logger.LogWarning("Price is null for region {Region}. Returning 0.", region);
                    return 0;
                }

                var rubPrice = await GetPrice(region, price);

                decimal priceWithMarkup = 0;
                // Fetch markup data from the repository
                var markupGame = (await _settingPriceRepository.GetAllList()).FirstOrDefault(p => p.Price >= rubPrice);
                var markupSub = (await _priceSettingSubscription.GetAllList()).FirstOrDefault(s => s.Region == region);

                if (markupGame == null || markupSub == null)
                {
                    _logger.LogError("Markup data not found for the given price: {Price}.", rubPrice);
                    throw new Exception("Markup data not found.");
                }

                switch (type)
                {
                    case "Game":
                    case "AddOn":
                        priceWithMarkup = rubPrice * markupGame.Price + rubPrice;
                        break;

                    case "Subscription":
                        priceWithMarkup = rubPrice * markupSub.Percent + rubPrice;
                        break;

                    default:
                        _logger.LogWarning("Unknown type: {Type}. Returning 0.", type);
                        priceWithMarkup = 0;
                        break;
                }

                _logger.LogInformation("Calculated price with markup: {PriceWithMarkup}", priceWithMarkup);
                return priceWithMarkup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for region {Region}, type {Type}.", region, type);
                throw;
            }
        }

        public async Task<decimal> CalcJprice(decimal? price, string region)
        {
            try
            {
                if (price == null)
                {
                    _logger.LogWarning("Price is null for region {Region}. Returning 0.", region);
                    return 0;
                }

                _logger.LogInformation("Calculating JPrice for price {Price} and region {Region}.", price, region);

                var loyality = (await _loyaltySettingRepository.GetAllList()).FirstOrDefault(l => l.PriceValue >= price.Value);
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

                var cashback = (await _cahsbackRepository.GetAllList()).FirstOrDefault();
                if (cashback == null)
                {
                    _logger.LogError("Cashback data not found.");
                    throw new KeyNotFoundException("Cashback setting not found");
                }

                decimal jPlus = price * cashback.Percent;
                _logger.LogInformation("Calculated JPlus: {JPlus}", jPlus);
                return jPlus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating JPlus for price {Price}.", price);
                throw;
            }
        }
    }
}
