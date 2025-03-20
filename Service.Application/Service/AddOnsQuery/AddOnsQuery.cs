using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using Service.Application.Service.AddOnsQuery.Dto;


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
            try
            {
                _logger.LogInformation("Fetching all addons.");
                var AddOns = await _groupAddOnRepository.GetAllList();

                result.AddRange((IEnumerable<AddOnsListDto>)Task.WhenAll(AddOns.Select(async a => new AddOnsListDto
                {
                    ProductId = (await _productRepository.GetEntityType(a.Guid)).Guid,
                    ImagePath = a.FilePathImage
                }

                )));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the add ons list.");
                throw;
            }

            return result;
        }

        public async Task<List<GroupAddOnsDto>> AddOnsList(Guid GroupAddOnId)
        {
            var result = new List<GroupAddOnsDto>();
            try
            {
                var groupAddOns = (await _groupAddOnRepository.GetListQuery()).Include(a => a.AddOns).ThenInclude(a => a.Product).FirstOrDefault(g => g.Guid == GroupAddOnId);
                if (groupAddOns is null) _logger.LogWarning("group add on with guid: {guid} is null", GroupAddOnId);
                else if (groupAddOns.AddOns is null) _logger.LogWarning("add ons in group add on with guid: {guid} is null", GroupAddOnId);
                var tasks = groupAddOns.AddOns.Select(async item => new GroupAddOnsDto
                {
                    ProductId = item.Product.Guid,
                    Image = item.Image,
                    Name = item.Name,
                    Price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type),
                    JPrice = await _calculatePrice.CalcJprice(await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type)),
                    Discount = item.Product.DiscountPercent
                });

                result.AddRange(await Task.WhenAll(tasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the group add on list.");
                throw;
            }
            return result;
        }


        public async Task<List<GameAddOnListDto>> GetGameAddOnList(Guid ProductId)
        {
            var result = new List<GameAddOnListDto>();
            try
            {
                var product = (await _productRepository.GetListQuery()).Include(p => p.Edition).ThenInclude(e => e.Game).ThenInclude(g => g.AddOns)
                    .FirstOrDefault(p => p.Guid == ProductId);

                result.AddRange(await Task.WhenAll(
                    product.Edition.Game.AddOns.Select(async item =>
                    new GameAddOnListDto
                    {
                        ProductId = (await _productRepository.GetEntityType(item.Guid)).Guid,
                        AddOnName = item.Name,
                        GameName = product.Edition.Game.Name,
                        Image = item.Image,
                        Platform = item.Platform,
                        Price = await _calculatePrice.CalcPrice((await _productRepository.GetEntityType(item.Guid)).PriceUa, (await _productRepository.GetEntityType(item.Guid)).PriceTr, (await _productRepository.GetEntityType(item.Guid)).Type),
                        JPrice = await _calculatePrice.CalcJprice(await _calculatePrice.CalcPrice((await _productRepository.GetEntityType(item.Guid)).PriceUa, (await _productRepository.GetEntityType(item.Guid)).PriceTr, (await _productRepository.GetEntityType(item.Guid)).Type)),
                        DiscountPercent = (await _productRepository.GetEntityType(item.Guid)).DiscountPercent
                    }
                    )));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

        }
    }
}
