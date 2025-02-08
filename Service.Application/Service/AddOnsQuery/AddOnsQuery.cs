using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;
using Service.Application.Service.AddOnsQuery.Dto;
using System.Linq;

namespace Service.Application.Service.AddOnsQuery
{
    public class AddOnsQuery
    {
        private readonly Repository<GroupAddOn> _groupAddOnRepository;
        private readonly GameRepository<Game> _gameRepository;
        private readonly Repository<AddOn> _addOnRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRegionFromCookie _regionFromCookie;
        public AddOnsQuery(ICalculationService calculatePrice, IHttpContextAccessor httpContextAccessor)
        {
            _calculatePrice = calculatePrice;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<AddOnsListDto>> GroupAddOnsList()
        {
            var result = new List<AddOnsListDto>();

            var AddOns = await _groupAddOnRepository.GetAllList();

            result.AddRange(AddOns.Select(a => new AddOnsListDto
            {
                Id = a.Guid,
                ImagePath = a.FilePathImage
            }

            ));

            return result;

        }

        public async Task<List<GroupAddOnsDto>> AddOnsList(Guid Id) // GroupAddOn ID
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            var result = new List<GroupAddOnsDto>();

            var groupAddOns = (await _groupAddOnRepository.GetAllList()).FirstOrDefault(g => g.Guid == Id);

            foreach (var item in groupAddOns.AddOns)
            {
                GroupAddOnsDto t = new GroupAddOnsDto();

                t.Id = item.Guid;
                t.Image = item.Image;
                t.Name = item.Name;
                if(item.Product.DiscountDate >= DateTime.UtcNow)
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
                    t.Discount = "0";
                    decimal? price = region switch
                    {
                        "UA" => item.Product.PriceUa,
                        "TR" => item.Product.PriceTr,
                        _ => throw new Exception("No region")

                    };
                    t.Price = await _calculatePrice.CalcPrice(price, item.Product.Type, region);
                    t.JPrice = await _calculatePrice.CalcJprice(t.Price, region);
                }
                result.Add(t);  
            }

            return result;
        }

        public async Task<AddOnDto> AddOnById(Guid Id)// Add on Id
        {
            var region = _regionFromCookie.GetUserRegion(_httpContextAccessor);

            var addOns = await _gameRepository.AddOnsByGame(Id);

            var addOn = await _addOnRepository.GetById(Id);
            var result = new AddOnDto();

            result.Id = Id;
            result.Image = addOn.Image;
            result.Type = addOn.TypeName ? "Pre Order" : "Donate";
            result.Platform = addOn.Platform;
            result.AddOns = addOns;
            if (addOn.Product.DiscountDate >= DateTime.UtcNow)
            {
               
                decimal? price = region switch
                {
                    "UA" => addOn.Product.DiscountUa,
                    "TR" => addOn.Product.DiscountTr,
                    _ => throw new Exception("No region")

                };
                result.Price = await _calculatePrice.CalcPrice(price, addOn.Product.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(result.Price, region);
            }
            else
            {
                decimal? price = region switch
                {
                    "UA" => addOn.Product.PriceUa,
                    "TR" => addOn.Product.PriceTr,
                    _ => throw new Exception("No region")

                };
                result.Price = await _calculatePrice.CalcPrice(price, addOn.Product.Type, region);
                result.JPrice = await _calculatePrice.CalcJprice(t.Price, region);
            }

            return result;
        }
    }
}
