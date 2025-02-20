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
using DataBaseToAccess.Repositiory;

namespace Services.ParseService
{
    public class Parse
    {
        private readonly ILogger<Parse> _logger;
        private readonly BaseDbContext _context;
        private readonly Repository<Game> _gameRepository;
        private readonly Repository<Edition> _editionRepository;
        private readonly Repository<Product> _productRepository;
        private readonly Repository<Geners> _genersRepository;
        public Parse(ILogger<Parse> logger, BaseDbContext context)
        {
            _logger = logger;
            _context = context;
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
            public string CusaCodeTR { get; set; }
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

        public async void StartParse(string json)
        {
            try
            {
                Dictionary<string, List<Guid>> keyValuePairs = new Dictionary<string, List<Guid>>();

                List<GameInfo>? games = JsonSerializer.Deserialize<List<GameInfo>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Учитываем регистр полей
                    AllowTrailingCommas = true, // Позволяем запятые в конце
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                if (games != null)
                {
                    _logger.LogInformation($"Всего игр загружено: {games.Count}");
                    foreach (var game in games)
                    {
                        var gameDto = new Game();

                        var productDto = new Product();

                        gameDto.Name = game.Name;
                        gameDto.ConceptId = game.ConceptId;
                        gameDto.Popular = game.StarCount.ToString();
                        bool rusTxt = game.LanguagesInterface.Contains("Русский");
                        bool rusVoice = game.LanguagesVoice.Contains("Русский");
                        if (rusTxt && rusVoice)
                        {
                            gameDto.Languages = "Полность на русском";
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
                        foreach (var edition in game.Editions)
                        {
                            var editionDto = new Edition();
                            editionDto.CusaCodeUa = edition.CusaCodeUA;
                            editionDto.CusaCodeTr = edition.CusaCodeTR;
                            editionDto.Type = edition.Type;
                            editionDto.EditionType = edition.EditionType;
                            editionDto.EditionName = edition.EditionName;
                            editionDto.Image = edition.Image;
                            editionDto.Platform = edition.Platform;
                            editionDto.Subscription = edition.Subscription;
                            editionDto.Region = edition.CodeRegion;
                            editionDto.Release = edition.Release;
                            editionDto.Game = gameDto;
                            editionDto.GameId = gameDto.Guid;
                            editionDto.ProductId = productDto.Guid;
                            editionDto.Geners = new List<Geners>();
                            foreach (var gener in edition.Geners.Split('|'))
                            {
                                keyValuePairs[gener].Add(editionDto.Guid);
                            }
                            productDto.TypeId = editionDto.Guid;
                            productDto.Type = edition.Type;
                            productDto.PriceUa = edition.Product.PriceUa;
                            productDto.PriceTr = edition.Product.PriceTr;
                            productDto.DiscountPercent = edition.Product.DiscountPercent;
                            productDto.DiscountDate = edition.Product.DiscountDate;

                            editionDto.Product = productDto;

                            gameDto.Editions.Add(editionDto);

                            await _editionRepository.Add(editionDto);
                            await _productRepository.Add(productDto);
                        }
                        await _gameRepository.Add(gameDto);
                    }

                    foreach(var key in keyValuePairs.Keys)
                    {
                        var g = new Geners
                        {
                            Name = key
                        };
                        await _genersRepository.Add(g);
                    }

                    var geners = await _genersRepository.GetAllList();
                    var editions = await _editionRepository.GetAllList();

                    foreach(var pair in keyValuePairs)
                    {
                        var gener = geners.FirstOrDefault(g => g.Name == pair.Key);
                        foreach(var id in pair.Value)
                        {
                            var edition = editions.FirstOrDefault(e => e.Guid == id);
                            edition.Geners.Add(gener);
                            await _editionRepository.Update(edition);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Загруженно 0 игр");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error occurred while parsing json");
                throw;
            }
        }


    }
}
