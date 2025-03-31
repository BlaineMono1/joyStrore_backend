using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DataBaseToAccess;
using Business.Data.Models;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

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
        IRepository<Subscription> subscriptionRepository
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



        public async Task CreateGameMarcup()
        {
            var l = new List<int> { 0, 100, 500, 1000, 1500, 2000, 4000, 6000, 10000 };

            foreach(var price in l)
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
                var role = new Role()
                {
                    Name = "User",
                    Users = new List<User>()
                };
                role.Users.Add(user);
                user.Cart = cart;
                user.CartId = cart.Guid;
                user.Favorite = fav;
                user.FavoriteId = fav.Guid;
                user.LoyaltyCurrency = l;
                user.ProductTransactionHistory = p;
                user.Role = role;
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
                                    EditionName = edition.EditionName,
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
                                productDto.DiscountPercent = edition.Product.DiscountPercent;
                                productDto.DiscountDate = edition.Product.DiscountDate is null ? null : DateTime.SpecifyKind((global::System.DateTime)edition.Product.DiscountDate, DateTimeKind.Utc);

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
                                        editionDto.EditionGeners.Add(new GenersToEdition { GenerId = g.Guid, Geners = g, EdtitonId = editionDto.Guid, Edition = editionDto});
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

    }
}