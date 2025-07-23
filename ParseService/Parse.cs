using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Business.Data.Models;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using System;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using System.Text;


namespace Services.ParseService
{
    public class Parse
    {
        private readonly ILogger<Parse> _logger;

        private readonly IRepository<Game> _gameRepository;
        private readonly IRepository<Edition> _editionRepository;
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
            PropertyNameCaseInsensitive = true
        };

        public Parse(ILogger<Parse> logger, IRepository<Game> gameRepository,
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
        IRepository<AddOn> addOnRepository
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

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://static.41.188.179.185.ip.webhost1.net:8080/")
            };
        }

        private class GameInfo
        {
            public string ConceptId { get; set; }
            public string Name { get; set; }
            public string LanguagesVoice { get; set; }
            public string LanguagesInterface { get; set; }
            public int StarCount { get; set; }
            public List<EditionInfo>? Editions { get; set; }
        }

        private class EditionInfo
        {
            public string CusaCodeUA { get; set; }
            public string? CusaCodeTR { get; set; }
            public string Type { get; set; }
            public string EditionType { get; set; }
            public string EditionName { get; set; }
            public string Geners { get; set; }
            public string Image { get; set; }
            public string Platform { get; set; }
            public string? Subscription { get; set; }
            public string Features { get; set; }
            public string CodeRegion { get; set; }
            public string OrderType { get; set; }
            public DateTime Release { get; set; }
            public ProductInfo Product { get; set; }
        }

        private class ProductInfo
        {
            public string Type { get; set; }
            public decimal? PriceUa { get; set; }
            public decimal? PriceTr { get; set; }
            public string DiscountPercent { get; set; }
            public DateTime? DiscountDate { get; set; }
        }

        public async Task CreatuSub()
        {
            var imageUrlPsPlus = "https://image.api.playstation.com/vulcan/ap/rnd/202204/0810/803Pm8uJoZ2Cl9fJPvTaXHqG.png";
            var imageUrlGtaPlus = "https://image.api.playstation.com/vulcan/ap/rnd/202310/1615/3d064be55673552147bde9d990e3b1251375b0f56a7dcfe3.png";

            var productPsPlus = new Product();

            var PsSub = new Subscription
            {
                CusaCodeTr = "CusacodeTr",
                CusaCodeUa = "CusacodeUa",
                Name = "PlayStation Plus",
                Type = "Subscription",
                Image = imageUrlPsPlus,
                Platform = "PS4|PS5",
                Duration = "1 Месяц",
                SectionName = "PlayStation Plus",
                ProductId = productPsPlus.Guid
            };

            productPsPlus.TypeId = PsSub.Guid;
            productPsPlus.Type = PsSub.Type;
            productPsPlus.PriceUa = 416;
            productPsPlus.PriceTr = 388;
            productPsPlus.DiscountPercentUa = "";
            productPsPlus.DiscountPercentTr = "";
            productPsPlus.DiscountDateUa = null;
            productPsPlus.DiscountDateTr = null;

            var psPriceUa = new PriceSettingSubscription
            {
                Region = "UAH",
                Percent = 0,
                SubscriptionId = PsSub.Guid
            };

            var psPriceTr = new PriceSettingSubscription
            {
                Region = "TRY",
                Percent = 0,
                SubscriptionId = PsSub.Guid
            };

            await _productRepository.Add(productPsPlus);
            await _subscriptionRepository.Add(PsSub);

            await _priceSettingSubscription.Add(psPriceUa);
            await _priceSettingSubscription.Add(psPriceTr);


            var productGtaPlus = new Product();

            var GtaSub = new Subscription
            {
                CusaCodeTr = "CusacodeTr",
                CusaCodeUa = "CusacodeUa",
                Name = "Gta Plus",
                Type = "Subscription",
                Image = imageUrlGtaPlus,
                Platform = "PS4|PS5",
                Duration = "1 Месяц",
                SectionName = "Gta Plus",
                ProductId = productGtaPlus.Guid
            };

            productGtaPlus.TypeId = PsSub.Guid;
            productGtaPlus.Type = PsSub.Type;
            productGtaPlus.PriceUa = 416;
            productGtaPlus.PriceTr = 388;
            productGtaPlus.DiscountPercentUa = "";
            productGtaPlus.DiscountPercentTr = "";
            productGtaPlus.DiscountDateUa = null;
            productGtaPlus.DiscountDateTr = null;

            var gtaPriceUa = new PriceSettingSubscription
            {
                Region = "UAH",
                Percent = 0,
                SubscriptionId = GtaSub.Guid
            };

            var gtaPriceTr = new PriceSettingSubscription
            {
                Region = "TRY",
                Percent = 0,
                SubscriptionId = GtaSub.Guid
            };

            await _productRepository.Add(productGtaPlus);
            await _subscriptionRepository.Add(GtaSub);

            await _priceSettingSubscription.Add(gtaPriceUa);
            await _priceSettingSubscription.Add(gtaPriceTr);

        }

        public async Task Create_addOn()
        {
            var product1 = new Product();

            var product2 = new Product();
            _logger.LogInformation(Guid.Parse("41c339a6-a6ca-4eed-ad94-0f4b245d9a37").ToString());
            var game = await _gameRepository.GetById(Guid.Parse("41c339a6-a6ca-4eed-ad94-0f4b245d9a37"));
            var add_on1 = new AddOn
            {
                CusaCodeUa = "CusaCodeUa",
                CusaCodeTr = "CusaCodeTr",
                TypeName = "AddOn",
                Name = "Red Dead Online - 150 Gold Bars",
                Type = "AddOn",
                Image = "https://image.api.playstation.com/cdn/EP1004/CUSA08519_00/jqNN0VH6CM4bKbwVGtqp85Mk4ZKU35w9.png",
                Platform = "PS4",
                GameId = game.Guid,
                ProductId = product1.Guid
            };

            var add_on2 = new AddOn
            {
                CusaCodeUa = "CusaCodeUa",
                CusaCodeTr = "CusaCodeTr",
                TypeName = "AddOn",
                Name = "Red Dead Online - 55 Gold Bars",
                Type = "AddOn",
                Image = "https://image.api.playstation.com/cdn/EP1004/CUSA08519_00/jqNN0VH6CM4bKbwVGtqp85Mk4ZKU35w9.png",
                Platform = "PS4",
                GameId = game.Guid,
                ProductId = product2.Guid
            };

            product1.TypeId = add_on1.Guid;
            product1.Type = "AddOn";
            product1.PriceUa = 2307;
            product1.PriceTr = 2168;
            product1.DiscountPercentUa = "0";
            product1.DiscountPercentTr = "0";
            product1.DiscountDateUa = null;
            product1.DiscountDateTr = null;


            product2.TypeId = add_on2.Guid;
            product2.Type = "AddOn";
            product2.PriceUa = 1154;
            product2.PriceTr = 1134;
            product2.DiscountPercentUa = "0";
            product2.DiscountPercentTr = "0";
            product2.DiscountDateUa = null;
            product2.DiscountDateTr = null;


            await _productRepository.Add(product1);
            await _productRepository.Add(product2);

            await _addOnRepository.Add(add_on1);
            await _addOnRepository.Add(add_on2);

        }

        public async Task CreateGameMarcup()
        {
            var l = new List<int> { 0, 100, 500, 1000, 1500, 2000, 4000, 6000, 10000 };

            foreach (var price in l)
            {
                var markup = new SettingPrice
                {
                    Price = price,
                    Percent = 0
                };

                await _settingPriceRepository.Add(markup);
            }
        }


        public async Task CreateSections()
        {
            //var edititons = (await _editionRepository.GetListQuery()).ToList();

            //int knt = 0;


            //int start = 0;
            //for(int i = 1; i < 3; ++i)
            //{

            //    var section = new Section
            //    {
            //        Name = $"section_{i}",
            //        FilePathImage = "IMAGEPATH",
            //        Editions = new List<Edition>()
            //    };

            //    await _sectionRepository.Add(section);
            //    for (int j = start; j < edititons.Count(); ++j)
            //    {
            //        section.Editions.Add(edititons[j]);
            //        start++;
            //        if (start % 3 == 0)
            //        {
            //            start++;
            //            break;
            //        }
            //    }
            //    await _sectionRepository.SaveDb();

            //}
        }
        public async Task RegUser()
        {
            try
            {
                var user = new User()
                {
                    TgUserId = "1",
                    Platform = "PS5"
                };
                var fav = new Favorite()
                {
                    UserId = user.Guid,
                    User = user
                };
                var cart = new Cart()
                {
                    User = user,
                    UserId = user.Guid
                };
                var l = new LoyaltyCurrency()
                {
                    User = user
                };
                var p = new ProductTransactionHistory()
                {
                    User = user
                };

                user.Cart = cart;
                user.CartId = cart.Guid;
                user.Favorite = fav;
                user.FavoriteId = fav.Guid;
                user.LoyaltyCurrency = l;
                user.ProductTransactionHistory = p;
                await _userRepo.Add(user);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task StartParse()
        {
            try
            {
                Dictionary<string, List<Guid>> keyValuePairs = new Dictionary<string, List<Guid>>();

                string filePath = "C:\\Users\\Danila\\Downloads\\Telegram Desktop\\cusacode.json"; // Укажите правильный путь
                List<GameInfo>? games = await ParseJsonFileAsync<List<GameInfo>>(filePath);

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

                    foreach (var ket in keyValuePairs.Keys)
                    {
                        var g = new Geners
                        {
                            Name = ket,
                            Editions = new List<GenersToEdition>()
                        };
                        await _genersRepository.Add(g);
                    }

                    _logger.LogInformation($"Всего игр загружено: {games.Count}");

                    foreach (var game in games)
                    {
                        var gameDto = new Game();


                        gameDto.Name = game.Name;
                        gameDto.ConceptId = game.ConceptId;
                        gameDto.Popular = game.StarCount.ToString();
                        bool rusTxt = game.LanguagesInterface.Contains("Русский");
                        bool rusVoice = game.LanguagesVoice.Contains("Русский");
                        if (rusTxt && rusVoice)
                        {
                            gameDto.Languages = "Полностью на русском";
                        }
                        else if (rusTxt)
                        {
                            gameDto.Languages = "Русский интерфейс";
                        }
                        else if (rusVoice)
                        {
                            gameDto.Languages = "Русская озвучка";
                        }
                        else
                        {
                            gameDto.Languages = "Не переведен на русский";
                        }

                        if (game.Editions != null) // Проверка на null
                        {
                            foreach (var edition in game.Editions)
                            {

                                if (edition == null) continue; // Пропуск, если edition равен null
                                var eg = edition.Geners.Split('|');
                                var geners = (await _genersRepository.GetListQuery()).AsTracking().Where(g => eg.Contains(g.Name));

                                var productDto = new Product();

                                var editionDto = new Edition
                                {
                                    CusaCodeUa = edition.CusaCodeUA,
                                    CusaCodeTr = edition.CusaCodeTR is null ? "" : edition.CusaCodeTR,
                                    Type = edition.Type,
                                    EditionType = edition.EditionType,
                                    Name = edition.EditionName,
                                    Image = edition.Image,
                                    Platform = edition.Platform,
                                    Subscription = edition.Subscription,
                                    Region = edition.CodeRegion,
                                    Release = DateTime.SpecifyKind(edition.Release, DateTimeKind.Utc),
                                    Game = gameDto,
                                    GameId = gameDto.Guid,
                                    ProductId = productDto.Guid,
                                    EditionGeners = new List<GenersToEdition>()
                                };




                                productDto.TypeId = editionDto.Guid;
                                productDto.Type = edition.Type;
                                productDto.PriceUa = edition.Product.PriceUa;
                                productDto.PriceTr = edition.Product.PriceTr;
                                // productDto.DiscountPercent = edition.Product.DiscountPercent;
                                //productDto.DiscountDate = edition.Product.DiscountDate is null ? null : DateTime.SpecifyKind((global::System.DateTime)edition.Product.DiscountDate, DateTimeKind.Utc);

                                editionDto.Product = productDto;


                                if (gameDto.Editions == null)
                                {
                                    gameDto.Editions = new List<Edition>();
                                }

                                gameDto.Editions.Add(editionDto);


                                foreach (var g in geners)
                                {

                                    if (!editionDto.EditionGeners.Any(e => e.GenerId == g.Guid))
                                    {
                                        editionDto.EditionGeners.Add(new GenersToEdition { GenerId = g.Guid, Geners = g, EdtitonId = editionDto.Guid, Edition = editionDto });
                                    }
                                }

                                await _editionRepository.Add(editionDto);

                            }

                        }
                        else
                        {
                            _logger.LogWarning($"Editions is null for game: {game.Name}");
                        }

                    }



                    var price = new SettingPrice
                    {
                        Price = 111111110M,
                        Percent = 0M
                    };

                    await _settingPriceRepository.Add(price);


                }
                else
                {
                    _logger.LogWarning("Загружено 0 игр");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error occurred while parsing json");
                throw;
            }
        }

        public static async Task<T?> ParseJsonFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден.");
                return default;
            }

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,  // Учитываем регистр полей
                    Converters = { new JsonStringEnumConverter() }, // Конвертация enum, если нужно
                    AllowTrailingCommas = true, // Разрешаем запятые в JSON
                };
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при разборе JSON: {ex.Message}");
                return default;
            }
        }

        public async Task<string> ParseAddOns(int startPage, int endPage)
        {
            string requestUri = $"addon-full?startPage={startPage}&endPage={endPage}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

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

            var cusacodes = (await _subscriptionRepository.GetListQuery()).Select(sub => sub.CusaCodeUa).ToList();

            //cusacodes.AddRange((await _editionRepository.GetListQuery()).Select(ed => ed.CusaCodeUa));

            cusacodes.AddRange((await _addOnRepository.GetListQuery()).Select(ed => ed.CusaCodeUa));

            const int BatchSize = 100;
            var allCodes = cusacodes;
            var data = new List<ResponceDto>();
            int updated = 0;

            foreach (var batch in allCodes
                       .Select((code, i) => new { code, i })
                       .GroupBy(x => x.i / BatchSize, x => x.code))
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(batch),
                    Encoding.UTF8,
                    "application/json"
                );
                var resp = await _httpClient.PostAsync("current-price", content);
                resp.EnsureSuccessStatusCode();
                var part = await resp.Content
                                    .ReadFromJsonAsync<List<ResponceDto>>(_jsonOptions) ?? new List<ResponceDto>();
                updated += part.Count;
                _logger.LogInformation($"Updated {updated} of {cusacodes.Count}");
                data.AddRange(part);
            }


            if (data == null)
                throw new InvalidOperationException("Сервер вернул пустой ответ.");


            foreach (var item in data)
            {
                var currentsub = (await _subscriptionRepository.GetListQuery()).FirstOrDefault(sub => sub.CusaCodeUa == item.CusaCodeUA);
                var currented = (await _editionRepository.GetListQuery()).FirstOrDefault(sub => sub.CusaCodeUa == item.CusaCodeUA);
                var currentadd = (await _addOnRepository.GetListQuery()).FirstOrDefault(sub => sub.CusaCodeUa == item.CusaCodeUA);

                if (currentsub != null)
                {
                    await UpdateProduct(item.ProductDto, currentsub.ProductId);

                    currentsub.Name = item.Name;
                    await _subscriptionRepository.Update(currentsub);
                }
                else if (currented != null)
                {
                    await UpdateProduct(item.ProductDto, currented.ProductId);

                    currented.Name = item.Name;
                    await _editionRepository.Update(currented);
                }
                else if (currentadd != null)
                {
                    await UpdateProduct(item.ProductDto, currentadd.ProductId);

                    currentadd.Name = item.Name;
                    await _addOnRepository.Update(currentadd);
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