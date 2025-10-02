using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Iterfaces;
using Service.Application.Service.AddOnsQuery;
using Service.Application.Service.GamesQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Service.Application.Service.GamesQuery
{
    public class GamesQuery
    {
        private readonly IRepository<Section> _sectionRepository;
        private readonly IGameRepository<Game> _gameRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IEditionRepository<Edition> _editionRepository;
        private readonly IRepository<AddOn> _addOnRepository;
        private readonly IRepository<GenersToEdition> _generToEditionsRepository;

        private readonly ICalculationService _calculatePrice;
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
            IRepository<AddOn> addOnRepository,
            IRepository<GenersToEdition> generToEditionsRepository
        )
        {
            _calculatePrice = calculatePrice;
            _regionFromCookie = regionFromCookie;
            _logger = logger;

            _sectionRepository = sectionRepository;
            _gameRepository = gameRepository;
            _productRepository = productRepository;
            _editionRepository = editionRepository;
            _addOnRepository = addOnRepository;
            _generToEditionsRepository = generToEditionsRepository;
        }

        public async Task<List<SectionDto>> GamesList()
        {
            var result = new List<SectionDto>();
            var region = _regionFromCookie.GetUserRegion();

            _logger.LogInformation("Fetching all sections.");
            var sections = (await _sectionRepository.GetListQuery())
                .Include(s => s.Products)
                .ThenInclude(se => se.Product)
                .OrderByDescending(s => s.DateCreate)
                .ToList();

            foreach (var section in sections)
            {
                if (section.Products is null || section.Products.Count < 1)
                {
                    _logger.LogWarning("Section {SectionName} has no products.", section.Name);
                    continue;
                }
                var sectionDto = new SectionDto { Name = section.Name };
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
                        Price = await _calculatePrice.CalcPrice(
                            product.PriceUa,
                            product.PriceTr,
                            product.Type
                        ),
                        Discount = (
                            region == "UAH" ? product.DiscountPercentUa : product.DiscountPercentTr
                        ),
                    };
                    dto.Jprice = await _calculatePrice.CalcJprice(dto.Price);

                    sectionDto.Editions.Add(dto);
                }
                result.Add(sectionDto);
            }

            return result;
        }

        public async Task AddEdition(
            string ConceptId,
            string Name,
            string Languages,
            string Popular,
            string NameEdition,
            string EditionType,
            string EditionName,
            string Image,
            string Platform,
            string Subscription,
            string Features,
            DateTime? Release,
            string Region,
            bool IsPreOrder,
            decimal PriceUa,
            decimal PriceTr,
            decimal DiscountPercentUa,
            decimal DiscountPercentTr,
            DateTime? DiscountDateUa,
            DateTime? DiscountDateTr,
            List<string> Geners,
            string CusaCodeUa,
            string CusaCodeTr,
            string Type
        )
        {
            var game = new Game();
            var edition = new Edition();
            var product = new Product();

            game.ConceptId = ConceptId;
            game.Name = Name;
            game.Languages = Languages;
            game.Popular = Popular;

            edition.CusaCodeTr = CusaCodeTr;
            edition.CusaCodeUa = CusaCodeUa;
            edition.EditionType = EditionType;
            edition.Type = EditionType;
            edition.Name = EditionName;
            edition.Image = Image;
            edition.Platform = Platform;
            edition.Subscription = Subscription;
            edition.Features = Features;
            edition.Region = Region;
            edition.Release = Release;
            edition.IsPreOrder = IsPreOrder;
            edition.GameId = game.Guid;
            edition.ProductId = product.Guid;

            product.PriceUa = PriceUa;
            product.PriceTr = PriceTr;
            product.DiscountPercentUa = DiscountPercentUa.ToString();
            product.DiscountPercentTr = DiscountPercentTr.ToString();
            product.DiscountDateUa = DiscountDateUa;
            product.DiscountDateTr = DiscountDateTr;
            product.Type = Type;
            product.TypeId = edition.Guid;

            await _productRepository.Add(product);
            await _gameRepository.Add(game);
            await _editionRepository.Add(edition);

            foreach (var generName in Geners)
            {
                var gener =
                    (await _gameRepository.GetListQuery()).FirstOrDefault(x => x.Name == generName)
                    ?? throw new NotFoundException(nameof(Geners), generName);

                var edg = new GenersToEdition { EdtitonId = edition.Guid, GenerId = gener.Guid };

                await _generToEditionsRepository.Add(edg);
            }
        }

        public async Task DeleteEdition(Guid EditionId)
        {
            var product = await _productRepository.GetEntityType(EditionId);

            await _productRepository.HardDelete(product.Guid);
            await _editionRepository.HardDelete(EditionId);
        }

        public async Task DeleteGame(Guid GameId)
        {
            var game =
                (await _gameRepository.GetListQuery())
                    .Include(g => g.Editions)
                    .Include(g => g.AddOns)
                    .FirstOrDefault(g => g.Guid == GameId)
                ?? throw new NotFoundException(nameof(Game), GameId);

            foreach (var ed in game.Editions ?? [])
            {
                await DeleteEdition(ed.Guid);
            }

            foreach (var ad in game.AddOns ?? [])
            {
                var prod = await _productRepository.GetEntityType(ad.Guid);
                await _productRepository.HardDelete(prod.Guid);
                await _addOnRepository.HardDelete(ad.Guid);
            }

            await _gameRepository.HardDelete(GameId);
        }
    }
}
