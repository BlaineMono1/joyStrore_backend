using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;
using Service.Application.Service.GamesQuery.Dto;

namespace Service.Application.Service.GamesQuery
{
    public class GetGamesList
    {
        private readonly Repository<Section> _sectionRepository;
        private readonly Repository<GroupAddOn> _addOnRepository;
        private readonly Repository<Game> _gameRepository;
        private readonly ProductRepository<Product> _productRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        public GetGamesList(ICalculationService calculatePrice, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<AddOnsListDto>> AddOnsList()
        {
            var result = new List<AddOnsListDto>();

            var AddOns = await _addOnRepository.GetAllList();

            result.AddRange(AddOns.Select(a => new AddOnsListDto
            {
                Id = a.Guid,
                ImagePath = a.FilePathImage
            }

            ));

            return result;
            
        }
    }
}
