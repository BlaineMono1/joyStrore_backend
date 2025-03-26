using System.Text.Json;
using Business.Data.BaseEntities;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;

namespace CacheService
{
    public class ExchangeRate : ICacheService
    {
        private readonly IRedisRepository _redis;
        private readonly ILogger<ExchangeRate> _logger;
        private readonly IRepository<LoyaltyCashback> _cashbackRepository;
        private static readonly HttpClient httpClient = new();

        public ExchangeRate(IRedisRepository redis, ILogger<ExchangeRate> logger, IRepository<LoyaltyCashback> cashbackRepository)
        {
            _logger = logger;
            _redis = redis;
            _cashbackRepository = cashbackRepository;
        }

              

        public async Task UpdateExchangeRates()
        {
            
            string cacheKeyUa = "UAH";
            string cacheKeyTr = "TRY";


            string urlUa = "https://min-api.cryptocompare.com/data/price?fsym=UAH&tsyms=RUB";
            string urlTr = "https://min-api.cryptocompare.com/data/price?fsym=TRY&tsyms=RUB";

            decimal Ua = await FetchExchangeRate(urlUa);
            decimal Tr = await FetchExchangeRate(urlTr);
            if(Ua == 0M || Tr == 0M)
            {
                throw new Exception($"Bad data from api  UAH-RUB = {Ua}, TRY-RUB = {Tr}");
            }
            var expireTime = TimeSpan.FromMinutes(10);

            await _redis.SetAsync(cacheKeyUa, Ua.ToString(), expireTime);
            await _redis.SetAsync(cacheKeyTr, Tr.ToString(), expireTime);

            _logger.LogInformation($"Обновлены курсы валют: UAH-RUB = {Ua}, TRY-RUB = {Tr}");
        }

        public async Task UpdateCashBack()
        {
            string cacheKey = "cashback";

            var entity = (await _cashbackRepository.GetListQuery()).First();

            string jsonData = JsonSerializer.Serialize(entity.Percent);

            await _redis.SetAsync(cacheKey, jsonData, null);

            _logger.LogInformation($"Закэширована таблица Loyality cashback с процентом кэшбэка {entity.Percent}");
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
