using System.Text.Json;
using Business.Data.Iterfaces.Store;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace CacheService
{
    public class ExchangeRate : BackgroundService
    {
        private readonly IRedisRepository _redis;
        private readonly ILogger<ExchangeRate> _logger;

        private static readonly HttpClient httpClient = new();

        public ExchangeRate(IRedisRepository redis, ILogger<ExchangeRate> logger)
        {
            _logger = logger;
            _redis = redis;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateExchangeRates();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении курсов валют.");
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task UpdateExchangeRates()
        {
            
            string cacheKeyUa = "UA";
            string cacheKeyTr = "TR";
         

            string urlUa = "https://min-api.cryptocompare.com/data/price?fsym=UAH&tsyms=RUB";
            string urlTr = "https://min-api.cryptocompare.com/data/price?fsym=TRY&tsyms=RUB";

            decimal Ua = await FetchExchangeRate(urlUa);
            decimal Tr = await FetchExchangeRate(urlTr);

            await _redis.SetAsync(cacheKeyUa, Ua.ToString());
            await _redis.SetAsync(cacheKeyTr, Tr.ToString());

            _logger.LogInformation($"Обновлены курсы валют: UAH-RUB = {Ua}, TRY-RUB = {Tr}");
        }

        private async Task<decimal> FetchExchangeRate(string url)
        {

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonResponse);
            return data != null && data.ContainsKey("RUB") ? (decimal)data["RUB"] : 0M;
        }
    }
}
