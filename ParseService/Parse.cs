using System;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Services.ParseService
{
    public class Parse
    {
        private readonly ILogger<Parse> _logger;

        private readonly IRepository<Game> _gameRepository;
        private readonly IRepository<Edition> _editionRepository;
        private readonly IRepository<GenersToEdition> _edRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IGenersRepository<Geners> _genersRepository;
        private readonly IRepository<SettingPrice> _settingPriceRepository;
        private readonly IRepository<LoyaltySetting> _loyaltySettingRepository;
        private readonly IRepository<LoyaltyCashback> _cahsbackRepository;
        private readonly IRepository<PriceSettingSubscription> _priceSettingSubscription;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Section> _sectionRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IRepository<AddOn> _addOnRepository;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public Parse(
            ILogger<Parse> logger,
            IRepository<Game> gameRepository,
            IRepository<Edition> editionRepository,
            IRepository<Product> productRepository,
            IGenersRepository<Geners> genersRepository,
            IRepository<SettingPrice> settingPriceRepository,
            IRepository<LoyaltySetting> loyaltySettingRepository,
            IRepository<LoyaltyCashback> cahsbackRepository,
            IRepository<PriceSettingSubscription> priceSettingSubscription,
            IRepository<User> userRepo,
            IRepository<Section> sectionRepository,
            IRepository<Subscription> subscriptionRepository,
            IRepository<AddOn> addOnRepository,
            IRepository<GenersToEdition> edRopository
        )
        {
            _logger = logger;

            _gameRepository = gameRepository;
            _editionRepository = editionRepository;
            _productRepository = productRepository;
            _genersRepository = genersRepository;
            _settingPriceRepository = settingPriceRepository;
            _loyaltySettingRepository = loyaltySettingRepository;
            _cahsbackRepository = cahsbackRepository;
            _priceSettingSubscription = priceSettingSubscription;
            _userRepo = userRepo;
            _sectionRepository = sectionRepository;
            _subscriptionRepository = subscriptionRepository;
            _addOnRepository = addOnRepository;
            _edRepository = edRopository;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://static.41.188.179.185.ip.webhost1.net:8080/"),
            };
            _httpClient.Timeout = TimeSpan.FromMinutes(50); // 50 mins, may be lower idk
        }

        public class AddOnInfo
        {
            [JsonPropertyName("conceptId")]
            public string ConceptId { get; set; }

            [JsonPropertyName("cusaCodeUA")]
            public string CusaCodeUA { get; set; }

            [JsonPropertyName("cusaCodeTR")]
            public string? CusaCodeTR { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("product")]
            public ProductInfo Product { get; set; }
        }

        public class GameInfo
        {
            [JsonPropertyName("conceptId")]
            public string ConceptId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("languagesVoice")]
            public string LanguagesVoice { get; set; }

            [JsonPropertyName("languagesInterface")]
            public string LanguagesInterface { get; set; }

            [JsonPropertyName("starCount")]
            public int StarCount { get; set; }

            [JsonPropertyName("editions")]
            public List<EditionInfo>? Editions { get; set; }
        }

        public class EditionInfo
        {
            [JsonPropertyName("cusaCodeUA")]
            public string CusaCodeUA { get; set; }

            [JsonPropertyName("cusaCodeTR")]
            public string? CusaCodeTR { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("editionType")]
            public string EditionType { get; set; }

            [JsonPropertyName("editionName")]
            public string EditionName { get; set; }

            [JsonPropertyName("geners")]
            public string Geners { get; set; }

            [JsonPropertyName("image")]
            public string Image { get; set; }

            [JsonPropertyName("platform")]
            public string Platform { get; set; }

            [JsonPropertyName("subscription")]
            public string? Subscription { get; set; }

            [JsonPropertyName("features")]
            public string Features { get; set; }

            [JsonPropertyName("codeRegion")]
            public string CodeRegion { get; set; }

            [JsonPropertyName("orderType")]
            public string OrderType { get; set; }

            [JsonPropertyName("release")]
            public string Release { get; set; }

            [JsonPropertyName("product")]
            public ProductInfo Product { get; set; }
        }

        public class ProductInfo
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("priceUa")]
            public decimal? PriceUa { get; set; }

            [JsonPropertyName("priceTr")]
            public decimal? PriceTr { get; set; }

            [JsonPropertyName("discountPercent")]
            public string DiscountPercent { get; set; }

            [JsonPropertyName("discountDate")]
            public DateTime? DiscountDate { get; set; }
        }

        public async Task PasrceAddOns(int startPage, int endPage)
        {
            string requestUri = $"addon-full?startPage={startPage}&endPage={endPage}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            string rawJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw JSON from API: {json}", rawJson);

            var addons =
                JsonSerializer.Deserialize<List<AddOnInfo>>(rawJson, _jsonOptions)
                ?? new List<AddOnInfo>();

            foreach (var addon in addons)
            {
                var game = (await _gameRepository.GetListQuery()).FirstOrDefault(g =>
                    g.ConceptId == addon.ConceptId
                );

                if (game == null)
                {
                    _logger.LogError($"No game for concept id {addon.ConceptId}");
                    continue;
                }

                var edition = game.Editions[0];

                var newAddOn = new AddOn
                {
                    CusaCodeTr = addon.CusaCodeTR,
                    CusaCodeUa = addon.CusaCodeUA,
                    TypeName = "Add-on",
                    Name = addon.Name,
                };
            }
        }

        public async Task ParseGames(int startPage, int endPage)
        {
            try
            {
                Dictionary<string, List<Guid>> keyValuePairs = new Dictionary<string, List<Guid>>();

                string requestUri = $"game-full?startPage={startPage}&endPage={endPage}";

                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

                response.EnsureSuccessStatusCode();

                string rawJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw JSON from API: {json}", rawJson);

                var games =
                    JsonSerializer.Deserialize<List<GameInfo>>(rawJson, _jsonOptions)
                    ?? new List<GameInfo>();

                if (games != null)
                {
                    foreach (var game in games)
                    {
                        foreach (var edition in game.Editions)
                        {
                            var g = edition.Geners.Split('|');
                            foreach (var e in g)
                            {
                                if (!keyValuePairs.ContainsKey(e))
                                {
                                    keyValuePairs.Add(e, new List<Guid>());
                                }
                            }
                        }
                    }

                    foreach (var key in keyValuePairs.Keys)
                    {
                        var gener = (await _genersRepository.GetListQuery()).FirstOrDefault(g =>
                            g.Name == key
                        );

                        if (gener is null)
                        {
                            var add = new Geners
                            {
                                Name = key,
                                Editions = new List<GenersToEdition>(),
                            };

                            await _genersRepository.Add(add);
                        }
                    }

                    _logger.LogInformation($"Всего игр загружено: {games.Count}");

                    foreach (var game in games)
                    {
                        var gameDto = new Game
                        {
                            Name = game.Name,
                            ConceptId = game.ConceptId,
                            Popular = game.StarCount.ToString(),
                            Languages = DetermineLanguage(
                                game.LanguagesInterface,
                                game.LanguagesVoice
                            ),
                        };

                        if (game.Editions != null)
                        {
                            foreach (var edition in game.Editions.Where(e => e != null))
                            {
                                var productDto = new Product
                                {
                                    Type = "Game",
                                    PriceUa = edition.Product.PriceUa ?? 0,
                                    PriceTr = edition.Product.PriceTr ?? 0,
                                    DiscountPercentUa = edition.Product.DiscountPercent,
                                    DiscountPercentTr = edition.Product.DiscountPercent,
                                    DiscountDateTr = edition.Product.DiscountDate ?? null,
                                    DiscountDateUa = edition.Product.DiscountDate ?? null,
                                };

                                var editionDto = new Edition
                                {
                                    CusaCodeUa = edition.CusaCodeUA,
                                    CusaCodeTr = edition.CusaCodeTR ?? string.Empty,
                                    Type = edition.Type,
                                    EditionType = edition.EditionType,
                                    Name = edition.EditionName,
                                    Image = edition.Image,
                                    Features = edition.Features,
                                    Platform = edition.Platform,
                                    Subscription = edition.Subscription,
                                    Region = edition.CodeRegion,
                                    Release = DateTime.SpecifyKind(
                                        DateTime.ParseExact(
                                            edition.Release ?? null,
                                            "d.M.yyyy",
                                            CultureInfo.InvariantCulture
                                        ),
                                        DateTimeKind.Utc
                                    ),
                                    Game = gameDto,
                                    GameId = gameDto.Guid,
                                    Product = productDto,
                                    ProductId = productDto.Guid,
                                    EditionGeners = new List<GenersToEdition>(),
                                };

                                var eg = edition.Geners.Split('|');
                                var geners = (await _genersRepository.GetListQuery())
                                    .AsTracking()
                                    .Where(g => eg.Contains(g.Name));

                                foreach (var g in geners)
                                {
                                    if (!editionDto.EditionGeners.Any(e => e.GenerId == g.Guid))
                                    {
                                        editionDto.EditionGeners.Add(
                                            new GenersToEdition
                                            {
                                                GenerId = g.Guid,
                                                Geners = g,
                                                EdtitonId = editionDto.Guid,
                                                Edition = editionDto,
                                            }
                                        );
                                    }
                                }

                                await _editionRepository.Add(editionDto);

                                productDto.TypeId = editionDto.Guid;

                                gameDto.Editions ??= new List<Edition>();
                                gameDto.Editions.Add(editionDto);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Editions is null for game: {game.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        // Вспомогательный метод для определения строки локализации
        private string DetermineLanguage(string iface, string voice)
        {
            bool rusTxt = iface.Contains("Русский");
            bool rusVoice = voice.Contains("Русский");
            return rusTxt && rusVoice ? "Полностью на русском"
                : rusTxt ? "Русский интерфейс"
                : rusVoice ? "Русская озвучка"
                : "Не переведен на русский";
        }

        // public async Task<string> ParseAddOns(int startPage, int endPage)
        // {
        //     string requestUri = $"addon-full?startPage={startPage}&endPage={endPage}";

        //     HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

        //     response.EnsureSuccessStatusCode();

        //     string rawJson = await response.Content.ReadAsStringAsync();
        //     _logger.LogInformation("Raw JSON from API: {json}", rawJson);

        //     var addons =
        //         JsonSerializer.Deserialize<List<AddOnInfo>>(rawJson, _jsonOptions)
        //         ?? new List<AddOnInfo>();

        //     foreach (var addon in addons)
        //     {
        //         var addOndto = new AddOn { };
        //         var product = new Product
        //         {
        //             Type = "Add=on",
        //             PriceUa = addon.Product.PriceUa ?? 0,
        //             PriceTr = addon.Product.PriceTr ?? 0,
        //             DiscountPercentUa = addon.Product.DiscountPercent,
        //             DiscountPercentTr = addon.Product.DiscountPercent,
        //             DiscountDateTr = addon.Product.DiscountDate,
        //             DiscountDateUa = addon.Product.DiscountDate,
        //         };
        //     }
        // }

        public class ProductDto
        {
            public string Type { get; set; }

            [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public decimal? PriceUa { get; set; }

            [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public decimal? PriceTr { get; set; }
            public string DiscountPercent { get; set; }
            public DateTime? DiscountDate { get; set; }
            public string DiscountPercentTr { get; set; }
            public DateTime? DiscountDateTr { get; set; }
        }

        public class ResponceDto
        {
            public string ConceptId { get; set; }
            public string CusaCodeUA { get; set; }
            public string CusaCodeTR { get; set; }
            public string Name { get; set; }
            public ProductDto ProductDto { get; set; }
        }

        public async Task UpdateProductsPrice()
        {
            var allSubscriptions = await _subscriptionRepository.GetAllList();
            var allEditions = await _editionRepository.GetAllList();
            var allAddOns = await _addOnRepository.GetAllList();

            var cusacodes = allSubscriptions
                .Select(sub => sub.CusaCodeUa)
                .Concat(allEditions.Select(ed => ed.CusaCodeUa))
                .Concat(allAddOns.Select(ed => ed.CusaCodeUa))
                .Distinct()
                .ToList();

            const int BatchSize = 100;
            var allCodes = cusacodes;
            var data = new List<ResponceDto>();
            int updated = 0;

            foreach (
                var batch in allCodes
                    .Select((code, i) => new { code, i })
                    .GroupBy(x => x.i / BatchSize, x => x.code)
            )
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(batch),
                    Encoding.UTF8,
                    "application/json"
                );
                var resp = await _httpClient.PostAsync("current-price", content);
                resp.EnsureSuccessStatusCode();
                var part =
                    await resp.Content.ReadFromJsonAsync<List<ResponceDto>>(_jsonOptions)
                    ?? new List<ResponceDto>();
                updated += part.Count;
                _logger.LogInformation($"Updated {updated} of {cusacodes.Count}");
                data.AddRange(part);
            }

            if (data == null)
                throw new InvalidOperationException("Сервер вернул пустой ответ.");

            var subscriptionDict = allSubscriptions.ToDictionary(s => s.CusaCodeUa, s => s);
            var editionDict = allEditions.ToDictionary(e => e.CusaCodeUa, e => e);
            var addOnDict = allAddOns.ToDictionary(a => a.CusaCodeUa, a => a);

            var productIds = new HashSet<Guid>(); // Добавляем этот HashSet
            foreach (var item in data)
            {
                if (subscriptionDict.TryGetValue(item.CusaCodeUA, out var sub) && sub != null)
                    productIds.Add(sub.ProductId);
                else if (editionDict.TryGetValue(item.CusaCodeUA, out var ed) && ed != null)
                    productIds.Add(ed.ProductId);
                else if (addOnDict.TryGetValue(item.CusaCodeUA, out var add) && add != null)
                    productIds.Add(add.ProductId);
            }

            var products = await (await _productRepository.GetListQuery())
                .Where(p => productIds.Contains(p.Guid))
                .ToListAsync();

            var productDict = products.ToDictionary(p => p.Guid, p => p);
            foreach (var item in data)
            {
                // var currentsub = (await _subscriptionRepository.GetListQuery()).FirstOrDefault(
                //     sub => sub.CusaCodeUa == item.CusaCodeUA
                // );
                // var currented = (await _editionRepository.GetListQuery()).FirstOrDefault(sub =>
                //     sub.CusaCodeUa == item.CusaCodeUA
                // );
                // var currentadd = (await _addOnRepository.GetListQuery()).FirstOrDefault(sub =>
                //     sub.CusaCodeUa == item.CusaCodeUA
                // );
                if (
                    subscriptionDict.TryGetValue(item.CusaCodeUA, out var currentsub)
                    && currentsub != null
                )
                {
                    if (productDict.TryGetValue(currentsub.ProductId, out var product))
                    {
                        // Обновляем данные продукта
                        await UpdateProduct(item.ProductDto, currentsub.ProductId);
                    }
                }
                else if (
                    editionDict.TryGetValue(item.CusaCodeUA, out var currented)
                    && currented != null
                )
                {
                    if (productDict.TryGetValue(currented.ProductId, out var product))
                    {
                        // Обновляем данные продукта
                        await UpdateProduct(item.ProductDto, currentsub.ProductId);
                    }
                }
                else if (
                    addOnDict.TryGetValue(item.CusaCodeUA, out var currentadd)
                    && currentadd != null
                )
                {
                    if (productDict.TryGetValue(currentadd.ProductId, out var product))
                    {
                        // Обновляем данные продукта
                        await UpdateProduct(item.ProductDto, currentsub.ProductId);
                    }
                }
                else
                {
                    _logger.LogError($"No product with CUSACODE UA {item.CusaCodeUA}");
                }
            }
        }

        private async Task UpdateProduct(ProductDto ProductInfo, Guid ProductId)
        {
            var product = await _productRepository.GetById(ProductId);

            product.PriceUa = ProductInfo.PriceUa;
            product.PriceTr = ProductInfo.PriceTr;
            product.DiscountPercentUa = ProductInfo.DiscountPercent;
            product.DiscountPercentTr = ProductInfo.DiscountPercentTr;

            await _productRepository.Update(product);
        }
    }
}
