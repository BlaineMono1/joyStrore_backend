using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.AddOnsQuery.Dto;
using System.Xml.Linq;
using static Service.Application.Exceptions.NotFoundExeption;


namespace Service.Application.Service.AddOnsQuery
{
    public class AddOnsQuery
    {
        private readonly IRepository<GroupAddOn> _groupAddOnRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IRepository<Game> _gameRepository;
        private readonly IRepository<AddOn> _addOnRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataFromCookie _regionFromCookie;
        private readonly ILogger<AddOnsQuery> _logger;
        public AddOnsQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IDataFromCookie regionFromCookie,
            ILogger<AddOnsQuery> logger,
            IRepository<GroupAddOn> groupAddOnRepository,
            IProductRepository<Product> productRepository,
            IRepository<Game> gameRepository,
            IRepository<AddOn> addOnRepository
            )
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;

            _groupAddOnRepository = groupAddOnRepository;
            _productRepository = productRepository;
            _gameRepository = gameRepository;
            _addOnRepository = addOnRepository;
        }
        public async Task<List<AddOnsListDto>> GroupAddOnsList()
        {
            var result = new List<AddOnsListDto>();


            var addOnsGroup = (await _groupAddOnRepository.GetListQuery()).Include(a => a.AddOns).ToList();

            foreach (var group in addOnsGroup)
            {
                var t = new AddOnsListDto
                {
                    ImagePath = group.FilePathImage,
                    GroupName = group.Name,
                    GroupAddOnId = group.Guid
                };

                result.Add(t);
            }


            return result;
        }

        public async Task<List<GroupAddOnsDto>> AddOnsList(Guid GroupAddOnId)
        {
            var region = _regionFromCookie.GetUserRegion();
            var result = new List<GroupAddOnsDto>();
            var groupAddOns = (await _groupAddOnRepository.GetListQuery()).Include(a => a.AddOns).FirstOrDefault(g => g.Guid == GroupAddOnId);
            if (groupAddOns is null) throw new NotFoundException(nameof(GroupAddOn), GroupAddOnId);


            foreach (var item in groupAddOns.AddOns)
            {
                var product = (await _productRepository.GetEntityType(item.Guid));
                var t = new GroupAddOnsDto
                {

                    ProductId = product.Guid,
                    Image = item.Image,
                    Name = item.Name,
                    Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type),
                    JPrice = await _calculatePrice.CalcJprice(await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type)),
                    Discount = (region == "UAH" ? product.DiscountPercentUa : product.DiscountPercentTr)
                };

                result.Add(t);
            }



            return result;
        }


        public async Task<List<GameAddOnListDto>> GetGameAddOnList(Guid ProductId)
        {
            var region = _regionFromCookie.GetUserRegion();
            var result = new List<GameAddOnListDto>();

            var product = (await _productRepository.GetListQuery()).Include(p => p.Edition).ThenInclude(e => e.Game).ThenInclude(g => g.AddOns)
                .FirstOrDefault(p => p.Guid == ProductId);

            if (product is null) throw new NotFoundException(nameof(Product), ProductId);

            foreach (var item in product.Edition.Game.AddOns)
            {
               
                var t = new GameAddOnListDto
                {

                    ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid,
                    AddOnName = item.Name,
                    GameName = product.Edition.Game.Name,
                    Image = item.Image,
                    Platform = item.Platform,
                    Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type),
                    DiscountPercent = (region == "UAH" ? product.DiscountPercentUa : product.DiscountPercentTr) ?? "0"
                };

                t.JPrice = await _calculatePrice.CalcJprice(t.Price);

                result.Add(t);
            }

            return result;


        }

        public async Task CreateAddOn(string CusaCodeUa, string CusaCodeTr, string TypeName, string Name, string Type, string Image, string Platform, Guid GroupAddOnId, 
            Guid GameId, decimal PriceUa, decimal PriceTr, decimal DiscountPercentUa, decimal DiscountPercentTr, DateTime? DiscountDateUa, DateTime? DiscountDateTr)
        {
            var addon = new AddOn();
            var product = new Product();
            var game = await _gameRepository.GetById(GameId) ?? throw new NotFoundException(nameof(Game), GameId);

            product.PriceUa = PriceUa;
            product.PriceTr = PriceTr;
            product.DiscountPercentUa = DiscountPercentUa.ToString();
            product.DiscountPercentTr = DiscountPercentTr.ToString();
            product.DiscountDateUa = DiscountDateUa;
            product.DiscountDateTr = DiscountDateTr;
            product.Type = Type;
            product.TypeId = addon.Guid;

            addon.CusaCodeUa = CusaCodeUa;
            addon.CusaCodeTr = CusaCodeTr;
            addon.TypeName = TypeName;
            addon.Name = Name;
            addon.Type = Type;
            addon.Image = Image;
            addon.Platform = Platform;
            addon.ProductId = product.Guid;
            addon.GameId = GameId;


            await _productRepository.Add(product);
            await _addOnRepository.Add(addon);
        }

        public async Task DeleteAddOn(Guid AddOnId)
        {
            var product = await _productRepository.GetEntityType(AddOnId);

            await _productRepository.HardDelete(product.Guid);
            await _addOnRepository.HardDelete(AddOnId);
        }
    }
}
