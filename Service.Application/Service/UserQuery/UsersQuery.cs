using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;
using Service.Application.Service.UserQuery.Dto;

namespace Service.Application.Service.UserQuery
{
    public class UsersQuery
    {
        private readonly UserRepository<User> _userRepository;
        private readonly Repository<Setting> _setingsRepository;
        private readonly Repository<LoyaltyCurrency> _loyalityRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        public UsersQuery(ICalculationService calculatePrice, IHttpContextAccessor httpContextAccessor)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserDto> UserByTgId(string tgId)
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);
            var user = await _userRepository.GetUserByTgId(tgId);
            
            var result = new UserDto();

            var settings = await _setingsRepository.GetById(user.Settings.FirstOrDefault(s => s.Region == region).Guid);

            var loyaloty = await _loyalityRepository.GetById(user.LoyaltyCurrencyId);

            result.Id = user.Guid;
            result.Email = settings.Email;
            result.Password = settings.PasswordPsStore;
            result.Code = settings.Code;
            result.JBal = loyaloty.BalanceJoy;
            result.JPlus = loyaloty.BalanceJoyPlus;
            result.Platform = settings.Platform;

            return result;
        }

        public async Task<List<CartDto>> UserCart(string tgId)
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            var user = await _userRepository.GetUserByTgId(tgId);

            var userCartItems = user.Cart.CartItems;

            List<CartDto> cart = [];

            if (userCartItems is null) { return cart; }

            foreach(var item in userCartItems)
            {
                var t = new CartDto()
                {
                    image = item.Product.Edition.Image,
                    Name = item.Product.Edition.Game.Name,
                    EditionName = item.Product.Edition.EditionName,

                };
                if (item.Product.DiscountDate >= DateTime.UtcNow)
                {
                    t.Discount = item.Product.DiscountPercent;
                    decimal? price = region switch
                    {
                        "UA" => item.Product.DiscountUa,
                        "TR" => item.Product.DiscountTr,
                        _ => throw new Exception("No region")

                    };
                    t.Price = await _calculatePrice.CalcPrice(price, item.Product.Type, region);
                    t.JPrice = await _calculatePrice.CalcJprice(t.Price, region);
                }
                else
                {
                    t.Discount = item.Product.DiscountPercent;
                    decimal? price = region switch 
                    {
                        "UA" => item.Product.PriceUa,
                        "TR" => item.Product.PriceTr,
                        _ => throw new Exception("No region")

                    };
                    t.Price = await _calculatePrice.CalcPrice(price, item.Product.Type, region);
                    t.JPrice = await _calculatePrice.CalcJprice(t.Price, region);
                }

                cart.Add(t);

            }

            return cart;
        }
    }
}
