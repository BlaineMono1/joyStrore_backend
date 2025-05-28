using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.AddOnsQuery.Dto;
using System.Xml.Linq;
using static Service.Application.Exceptions.NotFoundExeption;
using static System.Net.Mime.MediaTypeNames;


namespace Service.Application.Service.AddOnsQuery
{
    public class AddOnsQuery
    {
        private readonly IRepository<GroupAddOn> _groupAddOnRepository;
        private readonly IProductRepository<Product> _productRepository;


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
            IProductRepository<Product> productRepository
            )
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;

            _groupAddOnRepository = groupAddOnRepository;
            _productRepository = productRepository;
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
    }
}
