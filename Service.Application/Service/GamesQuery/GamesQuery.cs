using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Business.Data.Iterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Microsoft.EntityFrameworkCore;
using Service.Application.Service.GamesQuery.Dto;



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
                            Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type),
                            Discount = product.DiscountPercent
                        };
                        dto.Jprice = await _calculatePrice.CalcJprice(dto.Price);

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

         
    }
}
