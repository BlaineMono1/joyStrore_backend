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

namespace Services.ParseService
{
    public class Parse
    {
        private readonly ILogger<Parse> _logger;

        private readonly IRepository<Game> _gameRepository;
        private readonly IRepository<Edition> _editionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IGenersRepository<Geners> _genersRepository;
        public Parse(ILogger<Parse> logger, IRepository<Game> gameRepository,
        IRepository<Edition> editionRepository,
        IRepository<Product> productRepository,
        IGenersRepository<Geners> genersRepository)
        {
            _logger = logger;

            _gameRepository = gameRepository;
            _editionRepository = editionRepository;
            _productRepository = productRepository;
            _genersRepository = genersRepository;
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

        public async Task StartParse()
        {
            try
            {
                Dictionary<string, List<Guid>> keyValuePairs = new Dictionary<string, List<Guid>>();
                
                string filePath = "C:\\Users\\Danila\\Downloads\\Telegram Desktop\\cusacode.json"; // Укажите правильный путь
                List<GameInfo>? games = await ParseJsonFileAsync<List<GameInfo>>(filePath);

                if (games != null)
                {
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
                        await _gameRepository.Add(gameDto);
                        if (game.Editions != null) // Проверка на null
                        {
                            foreach (var edition in game.Editions)
                            {
                                if((await _gameRepository.GetAllList()).Count() == 633 && (await _editionRepository.GetAllList()).Count() == 1390)
                                {
                                    bool t = true; 
                                }
                                if (edition == null) continue; // Пропуск, если edition равен null
                                var productDto = new Product();
                                productDto.Guid = Guid.NewGuid();
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
                                    Geners = new List<Geners>()
                                };

                                if (edition.Geners != null) // Проверка на null
                                {
                                    foreach (var gener in edition.Geners.Split('|'))
                                    {
                                        if (!keyValuePairs.ContainsKey(gener))
                                        {
                                            keyValuePairs[gener] = new List<Guid>();
                                        }
                                        keyValuePairs[gener].Add(editionDto.Guid);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"Geners is null for edition: {edition.EditionName}");
                                }

                                if (edition.Product != null) // Проверка на null
                                {
                                    productDto.TypeId = editionDto.Guid;
                                    productDto.Type = edition.Type;
                                    productDto.PriceUa = edition.Product.PriceUa;
                                    productDto.PriceTr = edition.Product.PriceTr;
                                    productDto.DiscountPercent = edition.Product.DiscountPercent;
                                    productDto.DiscountDate = edition.Product.DiscountDate is null ? null : DateTime.SpecifyKind((global::System.DateTime)edition.Product.DiscountDate, DateTimeKind.Utc);

                                    editionDto.Product = productDto;
                                }
                                else
                                {
                                    _logger.LogWarning($"Product is null for edition: {edition.EditionName}");
                                }

                                if (gameDto.Editions == null)
                                {
                                    gameDto.Editions = new List<Edition>();
                                }

                                gameDto.Editions.Add(editionDto);

                                await _editionRepository.Add(editionDto);


                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Editions is null for game: {game.Name}");
                        }

                    }

                    foreach (var key in keyValuePairs.Keys)
                    {
                        var g = new Geners
                        {
                            Name = key
                        };
                        await _genersRepository.Add(g);
                    }

                    var editions = await _editionRepository.GetAllList();

                    foreach(var edition in editions)
                    {
                        if(edition.Geners is null) edition.Geners = new List<Geners>();
                        foreach(var pair in keyValuePairs)
                        {
                            var gener = await _genersRepository.GenerByName(pair.Key);
                            if (pair.Value.Contains(edition.Guid) && !edition.Geners.Any(g => g.Guid == gener.Guid)) edition.Geners.Add(gener);
                        }
                        await _editionRepository.Update(edition);
                    }
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