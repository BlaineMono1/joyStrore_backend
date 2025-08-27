using System.Text;
using System.Text.Json;
using System.Xml;
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
        private readonly IRepository<SettingPrice> _priceRepository;
        private static readonly HttpClient httpClient = new();

        public ExchangeRate(
            IRedisRepository redis,
            ILogger<ExchangeRate> logger,
            IRepository<LoyaltyCashback> cashbackRepository,
            IRepository<SettingPrice> priceRepository
        )
        {
            _logger = logger;
            _redis = redis;
            _cashbackRepository = cashbackRepository;
            _priceRepository = priceRepository;
        }

        public async Task UpdateExchangeRates()
        {
            string cacheKeyUa = "UAH";
            string cacheKeyTr = "TRY";

            decimal Ua = await FetchExchangeRate(cacheKeyUa);
            decimal Tr = await FetchExchangeRate(cacheKeyTr);
            if (Ua == 0M || Tr == 0M)
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

            _logger.LogInformation(
                $"Закэширована таблица Loyality cashback с процентом кэшбэка {entity.Percent}"
            );
        }

        public async Task UpdateMarkUp()
        {
            var percents = await _priceRepository.GetAllList();

            foreach (var p in percents)
            {
                var jsonData = JsonSerializer.Serialize(p.Percent);
                var key = $"MarkUpGame-{p.Price}";
                await _redis.SetAsync(key, jsonData, null);
                _logger.LogInformation(
                    $"Закэширована запись Setting price с ценой {p.Price} и наценкой {p.Percent}"
                );
            }
        }

        private async Task<decimal> FetchExchangeRate(string value)
        {
            string url = "https://www.cbr.ru/scripts/XML_daily.asp";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    byte[] rawBytes = await client.GetByteArrayAsync(url);

                    string xmlContent = Encoding.Default.GetString(rawBytes);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlContent);

                    XmlNode node = doc.SelectSingleNode($"//Valute[CharCode='{value}']");

                    if (node != null)
                    {
                        string unitRateStr = node["VunitRate"]?.InnerText;

                        return Convert.ToDecimal(unitRateStr);
                    }
                    else
                    {
                        throw new Exception($"Валюта {value} не найдена");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка получения курса валюты {value}: {ex.Message}");
                }
            }

            throw new Exception($"Не удалось получить курс для валюты {value}");
        }
    }
}
