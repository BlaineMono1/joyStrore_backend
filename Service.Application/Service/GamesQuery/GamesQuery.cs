using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;
using Service.Application.Service.GamesQuery.Dto;

namespace Service.Application.Service.GamesQuery
{
    public class GamesQuery
    {
        private readonly Repository<Section> _sectionRepository;
        private readonly Repository<Game> _gameRepository;
        private readonly ProductRepository<Product> _productRepository;
        private readonly EditionRepository<Edition> _editionRepository;
        private readonly GenersRepository<Geners> _genersRepository;
        private readonly UserRepository<User> _userRepository;


        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        public GamesQuery(ICalculationService calculatePrice, IHttpContextAccessor httpContextAccessor)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<GamesListDto>> GamesList()
        {
            var result = new List<GamesListDto>();

            var sections = await _sectionRepository.GetAllList();

            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            foreach (var section in sections)
            {
                var t = new GamesListDto();

                if (section.Editions is null || section.Editions.Count < 1) throw new Exception("Editions is null");

                foreach (var edition in section.Editions)
                {
                    var game = await _gameRepository.GetById(edition.GameId);

                    if (game is null) throw new Exception("Game is null");

                    t.FIlterName = section.Name;
                    t.Name = game.Name;
                    t.ImageFilepath = edition.Image;

                    t.Id = edition.Guid; // Id Edition or ID game?

                    var product = await _productRepository.GetEntityType(edition.Guid);

                    if (product.DiscountDate >= DateTime.UtcNow)
                    {
                        t.Discount = product.DiscountPercent;
                        decimal? price = region switch
                        {
                            "UA" => product.DiscountUa,
                            "TR" => product.DiscountTr,
                            _ => throw new Exception("No region")

                        };
                        t.Price = await _calculatePrice.CalcPrice(price, product.Type, region);
                        t.Jprice = await _calculatePrice.CalcJprice(t.Price, region);
                    }
                    else
                    {
                        t.Discount = "0";
                        decimal? price = region switch // Как регион хранится в куки??
                        {
                            "UA" => product.PriceUa,
                            "TR" => product.PriceTr,
                            _ => throw new Exception("No region")

                        };
                        t.Price = await _calculatePrice.CalcPrice(price, product.Type, region);
                        t.Jprice = await _calculatePrice.CalcJprice(t.Price, region);
                    }
                    result.Add(t);
                }

            }

            return result;
        }

        public async Task<GameDto> ShowGame(Guid GameId, Edition? Edition)
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            GameDto result = new GameDto();

            Edition ??=  (await _editionRepository.GetEditions(GameId)).FirstOrDefault();

            var editions = await _editionRepository.GetEditions(GameId);

            var product = await _productRepository.GetEntityType(GameId);
            
            var game = await _gameRepository.GetById(GameId);

            result.Id = GameId;
            result.Image = Edition.Image;
            result.Geners = await _genersRepository.GetGeners(Edition.Guid);
            result.RealiseDate = game.Release.Value;
            result.Platforms = Edition.Platform;
            result.Languages = game.Languages;
            result.Editions = editions;
            result.Subscription = Edition.Subscription;
            result.Discount = product.DiscountDate >= DateTime.UtcNow ? product.DiscountDate : null;
            result.Features = Edition.Features;
            
            if(result.Discount != null)
            {
                result.DiscountPercent = product.DiscountPercent;
                decimal? price = region switch
                {
                    "UA" => product.DiscountUa,
                    "TR" => product.DiscountTr,
                    _ => throw new Exception("No region")

                };
                result.Price = await _calculatePrice.CalcPrice(price, product.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
            }
            else
            {
                result.DiscountPercent = "0";
                decimal? price = region switch
                {
                    "UA" => product.PriceUa,
                    "TR" => product.PriceTr,
                    _ => throw new Exception("No region")

                };
                result.Price = await _calculatePrice.CalcPrice(price, product.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
            }

            result.JPlus = await _calculatePrice.CalcJplus(result.JPrice);

            var userTg = _regionFromCookie.GetUserTgID(_httpContextAccessor);
            var user = await _userRepository.GetUserByTgId(userTg);

            result.InCart = (user.Cart.CartItems.FirstOrDefault(c => c.ProductId == product.Guid) != null) ? true : false;
            result.InFavorite = (user.Favorite.FavoriteItems.FirstOrDefault(c => c.ProductId == product.Guid) != null) ? true : false;

            return result;
        }
    }
}
