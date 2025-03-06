using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Business.Data.Iterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.GamesQuery.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


namespace Service.Application.Service.GamesQuery
{
    public class GamesQuery
    {
        private readonly IRepository<Section> _sectionRepository;
        private readonly IGameRepository<Game> _gameRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IEditionRepository<Edition> _editionRepository;
        private readonly IGenersRepository<Geners> _genersRepository;
        private readonly IUserRepository<User> _userRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        private readonly ILogger<GamesQuery> _logger;

        public GamesQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IRegionFromCookie regionFromCookie,
            ILogger<GamesQuery> logger,
            IRepository<Section> sectionRepository,
            IGameRepository<Game> gameRepository,
            IProductRepository<Product> productRepository,
            IEditionRepository<Edition> editionRepository,
            IGenersRepository<Geners> genersRepository,
            IUserRepository<User> userRepository)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;

            _sectionRepository = sectionRepository;
            _gameRepository = gameRepository;
            _productRepository = productRepository;
            _editionRepository = editionRepository;
            _userRepository = userRepository;
        }

        public async Task<List<GamesListDto>> GamesList()
        {
            var result = new List<GamesListDto>();

            try
            {
                _logger.LogInformation("Fetching all sections.");
                var sections = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).ToList();

                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

                foreach (var section in sections)
                {
                    if (section.Editions is null || section.Editions.Count < 1)
                    {
                        _logger.LogWarning("Section {SectionName} has no editions.", section.Name);
                        continue;
                    }

                    foreach (var edition in section.Editions)
                    {
                        var game = await _gameRepository.GetById(edition.GameId);

                        if (game is null)
                        {
                            _logger.LogError("Game with ID {GameId} not found.", edition.GameId);
                            continue;
                        }

                        var product = await _productRepository.GetEntityType(edition.Guid);

                        var dto = new GamesListDto
                        {
                            FIlterName = section.Name,
                            Name = game.Name,
                            ImageFilepath = edition.Image,
                            Id = game.Guid,
                            Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type, region),
                            Discount = product.DiscountPercent
                        };
                        dto.Jprice = await _calculatePrice.CalcJprice(dto.Price, region);

                        result.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the games list.");
                throw;
            }

            return result;
        }

        public async Task<GameDto> ShowGame(Guid GameId, Guid Edition)
        {
            try
            {
                _logger.LogInformation("Fetching game details for GameId: {GameId}, Edition: {Edition}", GameId, Edition);

                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

                var edition = (await _editionRepository.GetEditions(GameId)).FirstOrDefault(e => e.Guid == Edition);
                if (edition == null)
                {
                    _logger.LogWarning("Edition {Edition} not found for GameId {GameId}.", Edition, GameId);
                    throw new Exception("Edition not found.");
                }

                var editions = new List<EditionDto>(); 
                editions.AddRange((await _editionRepository.GetEditions(GameId)).Select(item => 
                new EditionDto()
                {
                    Id = item.Guid,
                    Name = item.EditionName
                }));
                var product = await _productRepository.GetEntityType(Edition);
                var game = (await _gameRepository.GetListQuery()).Include(g => g.AddOns).ThenInclude(a => a.Product).FirstOrDefault(g => g.Guid == GameId);

                if (game == null)
                {
                    _logger.LogError("Game with ID {GameId} not found.", GameId);
                    throw new Exception("Game not found.");
                }

                var addOns = new List<AddOnDto>();

                var task = (game.AddOns.Select(async item =>
                new AddOnDto
                {
                    Id = item.Guid,
                    AddOnName = item.Name,
                    GameName = game.Name,
                    Image = item.Image,
                    Platform = item.Platform,
                    DiscountPercent = item.Product.DiscountPercent,
                    Price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type, region),
                    JPrice = await _calculatePrice.CalcJprice(await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type, region), region)
                }
                ));

                addOns.AddRange(await Task.WhenAll(task)); 

                var result = new GameDto
                {
                    Id = GameId,
                    Image = edition.Image,
                    Geners =edition.EditionGeners.Select(g => g.Geners.Name).ToList(),
                    RealiseDate = edition.Release,
                    Platforms = edition.Platform,
                    Languages = game.Languages,
                    Editions = editions,
                    Subscription = edition.Subscription ?? "",
                    Discount = product.DiscountDate,
                    DiscountPercent = product.DiscountPercent,
                    Features = edition.Features ?? "",
                    Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type, region),
                    Addons = addOns
                };

                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
                result.JPlus = await _calculatePrice.CalcJplus(result.JPrice);

                var userTg = _regionFromCookie.GetUserTgID(_httpContextAccessor);
                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).Include(u => u.Favorite).ThenInclude(f => f.FavoriteItems).FirstOrDefault(u => u.TgUserId == userTg);

                result.InCart = user.Cart.CartItems.Any(c => c.ProductId == product.Guid);
                result.InFavorite = user.Favorite.FavoriteItems.Any(c => c.ProductId == product.Guid);
                result.IsPlatform = result.Platforms.Contains(user.Platform); 
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving game details for GameId: {GameId}, Edition: {Edition}", GameId, Edition);
                throw;
            }
        }

        public async Task<List<GamesListDto>> FilterGames(string? name, List<string>? geners)
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
            try
            {
                _logger.LogInformation("Filtering games");
                var editions = await _editionRepository.FilterEditions(name, geners);

                var result = new List<GamesListDto>();

                foreach (var edition in editions)
                {
                    var t = new GamesListDto
                    {
                        FIlterName = name,
                        Id = edition.Guid,
                        ImageFilepath = edition.Image,
                        Name = edition.EditionName,
                        Discount = edition.Product.DiscountPercent,
                        Price = await _calculatePrice.CalcPrice(edition.Product.PriceUa, edition.Product.PriceTr, edition.Product.Type, region),
                        
                    };
                    t.Jprice = await _calculatePrice.CalcJprice(t.Price, region);
                    result.Add(t);
                }

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while filtering games");
                throw;
            }
        }
    }
}
