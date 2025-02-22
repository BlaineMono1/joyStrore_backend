using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.GamesQuery.Dto;
using static System.Collections.Specialized.BitVector32;

namespace Service.Application.Service.GamesQuery
{
    public class GamesQuery
    {
        private readonly Repository<Section> _sectionRepository;
        private readonly GameRepository<Game> _gameRepository;
        private readonly ProductRepository<Product> _productRepository;
        private readonly EditionRepository<Edition> _editionRepository;
        private readonly GenersRepository<Geners> _genersRepository;
        private readonly UserRepository<User> _userRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        private readonly ILogger<GamesQuery> _logger;

        public GamesQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IRegionFromCookie regionFromCookie,
            ILogger<GamesQuery> logger)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;
        }

        public async Task<List<GamesListDto>> GamesList()
        {
            var result = new List<GamesListDto>();

            try
            {
                _logger.LogInformation("Fetching all sections.");
                var sections = await _sectionRepository.GetAllList();

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

                var editions = await _editionRepository.GetEditions(GameId);
                var product = await _productRepository.GetEntityType(GameId);
                var game = await _gameRepository.GetById(GameId);

                if (game == null)
                {
                    _logger.LogError("Game with ID {GameId} not found.", GameId);
                    throw new Exception("Game not found.");
                }

                var result = new GameDto
                {
                    Id = GameId,
                    Image = edition.Image,
                    Geners =edition.Geners,
                    RealiseDate = edition.Release,
                    Platforms = edition.Platform,
                    Languages = game.Languages,
                    Editions = editions,
                    Subscription = edition.Subscription,
                    Discount = product.DiscountDate,
                    DiscountPercent = product.DiscountPercent,
                    Features = edition.Features,
                    Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type, region),
                    Addons = game.AddOns
                };

                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
                result.JPlus = await _calculatePrice.CalcJplus(result.JPrice);

                var userTg = _regionFromCookie.GetUserTgID(_httpContextAccessor);
                var user = await _userRepository.GetUserByTgId(userTg);

                result.InCart = user.Cart.CartItems.Any(c => c.ProductId == product.Guid);
                result.InFavorite = user.Favorite.FavoriteItems.Any(c => c.ProductId == product.Guid);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving game details for GameId: {GameId}, Edition: {Edition}", GameId, Edition);
                throw;
            }
        }

        public async Task<List<GamesListDto>> FilterGames(string name, List<string> geners)
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
            try
            {
                _logger.LogInformation("Filtering games");
                var games = await _gameRepository.FilterGames(name, geners);

                var result = await Task.WhenAll(
                    games.SelectMany(game => game.Editions)
                         .Select(async edition => new GamesListDto
                         {
                             FIlterName = name,
                             Id = edition.Game.Guid,
                             ImageFilepath = edition.Image,
                             Name = edition.EditionName,
                             Discount = edition.Product.DiscountPercent,
                             Price = await _calculatePrice.CalcPrice(edition.Product.PriceUa, edition.Product.PriceTr, edition.Product.Type, region),
                             Jprice = await _calculatePrice.CalcJprice(
                                 await _calculatePrice.CalcPrice(edition.Product.PriceUa, edition.Product.PriceTr, edition.Product.Type, region),
                                 region)
                         })
                );

                return result.ToList();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while filtering games");
                throw;
            }
        }
    }
}
