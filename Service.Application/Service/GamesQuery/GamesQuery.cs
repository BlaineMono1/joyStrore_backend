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
        private readonly IRepository<AddOn> _addOnRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataFromCookie _regionFromCookie;
        private readonly ILogger<GamesQuery> _logger;

        public GamesQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IDataFromCookie regionFromCookie,
            ILogger<GamesQuery> logger,
            IRepository<Section> sectionRepository,
            IGameRepository<Game> gameRepository,
            IProductRepository<Product> productRepository,
            IEditionRepository<Edition> editionRepository,
            IGenersRepository<Geners> genersRepository,
            IUserRepository<User> userRepository,
            IRepository<AddOn> addOnRepository
            )
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
            _addOnRepository = addOnRepository;
        }

        public async Task<List<SectionDto>> GamesList()
        {
            var result = new List<SectionDto>();
            var region = _regionFromCookie.GetUserRegion();

            _logger.LogInformation("Fetching all sections.");
            var sections = (await _sectionRepository.GetListQuery()).Include(s => s.Products).ThenInclude(se => se.Product).ToList();


            foreach (var section in sections)
            {
                if (section.Products is null || section.Products.Count < 1)
                {
                    _logger.LogWarning("Section {SectionName} has no products.", section.Name);
                    continue;
                }
                var sectionDto = new SectionDto
                {
                    Name = section.Name,
                };
                foreach (var sectionProduct in section.Products)
                {

                    var product = sectionProduct.Product;

                    var addOn = (await _addOnRepository.GetById(product.TypeId));
                    var edition = (await _editionRepository.GetById(product.TypeId));

                    var dto = new GamesListDto
                    {
                        Name = product.Type == "Game" ? edition.Name : addOn.Name,
                        ImageFilepath = product.Type == "Game" ? edition.Image : addOn.Image,
                        ProductId = product.Guid,
                        Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type),
                        Discount = (region == "UAH" ? product.DiscountPercentUa : product.DiscountPercentTr)
                    };
                    dto.Jprice = await _calculatePrice.CalcJprice(dto.Price);

                    sectionDto.Editions.Add(dto);
                }
                result.Add(sectionDto);

            }


            return result;
        }


    }
}
