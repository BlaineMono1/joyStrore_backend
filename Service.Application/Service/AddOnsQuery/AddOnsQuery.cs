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
        private readonly IGameRepository<Game> _gameRepository;
        private readonly IRepository<AddOn> _addOnRepository;
        private readonly IUserRepository<User> _userRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        private readonly ILogger<AddOnsQuery> _logger;
        public AddOnsQuery(
            ICalculationService calculatePrice,
            IHttpContextAccessor httpContextAccessor,
            IRegionFromCookie regionFromCookie,
            ILogger<AddOnsQuery> logger,
            IRepository<GroupAddOn> groupAddOnRepository,
            IGameRepository<Game> gameRepository,
            IRepository<AddOn> addOnRepository,
            IUserRepository<User> userRepository)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
            _regionFromCookie = regionFromCookie;
            _logger = logger;

            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _groupAddOnRepository = groupAddOnRepository;
        }
        public async Task<List<AddOnsListDto>> GroupAddOnsList()
        {
            var result = new List<AddOnsListDto>();
            try
            {
                _logger.LogInformation("Fetching all addons.");
                var AddOns = await _groupAddOnRepository.GetAllList();

                result.AddRange(AddOns.Select(a => new AddOnsListDto
                {
                    Id = a.Guid,
                    ImagePath = a.FilePathImage
                }

                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the add ons list.");
                throw;
            }

            return result;
        }

        public async Task<List<GroupAddOnsDto>> AddOnsList(Guid Id) // GroupAddOn ID
        {
            var result = new List<GroupAddOnsDto>();
            try
            {
                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
                
                var groupAddOns = (await _groupAddOnRepository.GetListQuery()).Include(a => a.AddOns).ThenInclude(a => a.Product).FirstOrDefault(g => g.Guid == Id);
                if (groupAddOns is null) _logger.LogWarning("group add on with guid: {guid} is null", Id);
                else if (groupAddOns.AddOns is null) _logger.LogWarning("add ons in group add on with guid: {guid} is null", Id);
                var tasks = groupAddOns.AddOns.Select(async item => new GroupAddOnsDto
                {
                    Id = item.Guid,
                    Image = item.Image,
                    Name = item.Name,
                    Price = await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type, region),
                    JPrice = await _calculatePrice.CalcJprice(await _calculatePrice.CalcPrice(item.Product.PriceUa, item.Product.PriceTr, item.Product.Type, region), region),
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

        public async Task<AddOnDto> AddOnById(Guid Id)// Add on Id
        {
            var result = new AddOnDto();
            try
            {
                var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

                var addOns = await _gameRepository.AddOnsByGame(Id);
                if (addOns is null) _logger.LogWarning("No game with add on {guid}", Id);

                var addOn = await _addOnRepository.GetById(Id);
                if (addOns is null) _logger.LogWarning("add on {guid} not found", Id);

                result.Id = Id;
                result.Image = addOn.Image;
                result.Type = addOn.TypeName;
                result.Platform = addOn.Platform;
                result.AddOns = addOns;
                result.Price = await _calculatePrice.CalcPrice(addOn.Product.PriceUa, addOn.Product.PriceTr, addOn.Product.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
                result.JPlus = await _calculatePrice.CalcJplus(result.JPrice);

                var userTg = _regionFromCookie.GetUserTgID(_httpContextAccessor);
                var user = (await _userRepository.GetListQuery()).Include(u => u.Cart).ThenInclude(c => c.CartItems).Include(u => u.Favorite).ThenInclude(f => f.FavoriteItems).Include(u => u.Settings).FirstOrDefault(u => u.TgUserId == userTg);

                result.InCart = (user.Cart.CartItems.FirstOrDefault(c => c.ProductId == addOn.Product.Guid) != null) ? true : false;
                result.InFavorite = (user.Favorite.FavoriteItems.FirstOrDefault(c => c.ProductId == addOn.Product.Guid) != null) ? true : false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving add on.");
                throw;
            }
            return result;
        }
    }
}
