using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using System.Text.Json;


namespace Service.Application.Service.CalculationService
{
    public class CalculatePrice
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Repository<SettingPrice> _settingPriceRepository;
        private readonly Repository<PriceSettingSubscription> _priceSettingSubscription;
        private readonly Repository<LoyaltySetting> _loyaltySettingRepository;
        private static async Task<decimal> GetExchangeRateUA()
        {
            string url = "https://min-api.cryptocompare.com/data/price?fsym=UAH&tsyms=RUB";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonResponse);

            return data != null && data.ContainsKey("RUB")
                ? (decimal)Math.Round(data["RUB"], MidpointRounding.AwayFromZero)
                : throw new Exception("Invalid response from API");
        }

        private static async Task<decimal> GetExchangeRateTR()
        {
            string url = "https://min-api.cryptocompare.com/data/price?fsym=TRY&tsyms=RUB";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonResponse);

            return data != null && data.ContainsKey("RUB")
                ? (decimal)Math.Round(data["RUB"], MidpointRounding.AwayFromZero)
                : throw new Exception("Invalid response from API");
        }


        private async Task<decimal> GetPrice(string region, decimal? price)
        {
            if (price == null) return 0;

            decimal exchangeRate = region switch
            {
                "TR" => await GetExchangeRateTR(),
                "UA" => await GetExchangeRateUA(),
                _ => throw new Exception("No region found")
            };

            return price.Value * exchangeRate;
        }

        public async Task<decimal> CalcPrice(decimal? price, string type, string region)
        {
            if (price == null) return 0;

            var rubPrice =  type switch
            {
                "Game" => await GetPrice(region, price),
                "AddOn" => await GetPrice(region, price),
                "Subscription" => await GetPrice(region, price),
                _ => throw new KeyNotFoundException($"Type '{type}' is not found.")
            };

            decimal priceWithMarkup = 0;
            var markupGame = (await _settingPriceRepository.GetAllList()).FirstOrDefault(p => p.Price >= rubPrice);
            var markupSub = (await _priceSettingSubscription.GetAllList()).FirstOrDefault(s => Enum.GetName(typeof(Region), s.Region) == region);
            switch (type)
            {
                case "Game":
                    
                    priceWithMarkup = rubPrice * markupGame.Price + rubPrice;
                    break;

                case "AddOn":
                    
                    priceWithMarkup = rubPrice * markupGame.Price + rubPrice;
                    break;

                case "Subscription":
                    priceWithMarkup = rubPrice * markupSub.Percent + rubPrice;
                    break;

                default:
                    priceWithMarkup = 0;
                    break;
            }

            return priceWithMarkup;
        }

        public async Task<decimal> CalcJprice(decimal? price, string region)
        {
            if (price == null) return 0;

            var loyality = (await _loyaltySettingRepository.GetAllList()).FirstOrDefault(l => l.PriceValue >= price.Value);
            if (loyality == null) throw new KeyNotFoundException();
            return price.Value - (price.Value * loyality.DiscountPercent);
        }
    }
}
