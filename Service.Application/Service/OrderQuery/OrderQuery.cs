
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Service.Application.Iterfaces;
using Service.Application.Service.OrderQuery.Dto;
using System.Xml.Linq;

namespace Service.Application.Service.OrderQuery
{
    public class OrderQuery
    {
        private readonly IUserRepository<User> _userRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IRepository<Setting> _settingRepository;

        private readonly ICalculationService _calculatePrice;
        private readonly IDataFromCookie _regionFromCookie;
        public OrderQuery(IUserRepository<User> userRepository, ICalculationService calculatePrice, IDataFromCookie regionFromCookie, 
                         IRepository<Cart> cartRepository, IProductRepository<Product> productRepository, IRepository<Order> orderRepository, 
                         IRepository<CartItem> cartItemRepository, IRepository<Setting> settingRepository)
        {
            _userRepository = userRepository;
            _calculatePrice = calculatePrice;
            _regionFromCookie = regionFromCookie;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _settingRepository = settingRepository;
        }

        public async Task<OrdersDto> CreateOrder(bool isTokenPayment) //Создание ордера в бд
        {

            var region = _regionFromCookie.GetUserRegion();
            var userTgId = _regionFromCookie.GetUserTgID();

            var result = new OrdersDto
            {
                Region = region,
                TgUserId = userTgId,
                Products = new List<OrderItemDto>()
            };
            
            var order = new Order
            {
                OrderProductItems = new List<OrderProductItem>(),
                Status = Business.Data.Enums.OrderStatus.Created,
            };
            order.OrderCode = GenerateCode(order.Guid);
            order.TgUserId = userTgId;

            var user = await _userRepository.GetUserByTgId(userTgId);

            var cart = (await _cartRepository.GetListQuery()).Include(c => c.CartItems).First(c => c.UserId == user.Guid);

            if(cart.CartItems is null || cart.CartItems.Count == 0)
            {
                throw new Exception("Cart is empty");
            }
            decimal price = 0, jPrice = 0;
            foreach (var item in cart.CartItems)
            {
                var product = await _productRepository.GetById(item.ProductId);

                if (product is null) continue;

                var orderItem = new OrderProductItem
                {
                    Price = await _calculatePrice.CalcPrice(product.PriceUa, product.PriceTr, product.Type),
                    Discount = product.DiscountPercent,                    
                    OrderId = order.Guid,
                    ProductId = product.Guid,
                };

                orderItem.JPrice = await _calculatePrice.CalcJprice(orderItem.Price);
                string name = "", cusacode = "";
                switch (product.Type)
                {
                    case "Game":
                        var edition = await _productRepository.GetTypeEntity<Edition>(product);
                        name = edition.Name;
                        cusacode = (region == "UAH" ? edition.CusaCodeUa : edition.CusaCodeTr);
                        break;
                    case"AddOn":
                        var addOn = await _productRepository.GetTypeEntity<AddOn>(product);
                        name = addOn.Name;
                        cusacode = (region == "UAH" ? addOn.CusaCodeUa : addOn.CusaCodeTr);
                        break;
                    case "Subscription":
                        var sub = await _productRepository.GetTypeEntity<Subscription>(product);
                        name = sub.Name;
                        cusacode = (region == "UAH" ? sub.CusaCodeUa : sub.CusaCodeTr);
                        break;
                };

                var orderItemDto = new OrderItemDto
                {
                    Name = name,
                    CusaCode = cusacode,
                    Type = product.Type
                };

                if(isTokenPayment)
                {
                    jPrice += orderItem.JPrice;
                }
                else
                {
                    price += orderItem.Price;
                }

                orderItemDto.Price = (isTokenPayment ? orderItem.JPrice : orderItem.Price);
                result.Products.Add(orderItemDto);

                order.OrderProductItems.Add(orderItem);
                await _cartItemRepository.HardDelete(item.Guid);

            }


            order.Price = price;
            order.JPrice = jPrice;


            result.OrderCode = order.OrderCode;
            result.TotalPrice = (isTokenPayment ? order.JPrice : order.Price);
            result.UserPaid = result.TotalPrice; //Заглушка
            result.Status = order.Status.ToString();

            var userSettings = (await _settingRepository.GetListQuery()).First(s => s.UserId == user.Guid && s.Region == region);

            result.PsLogin = userSettings.EmailPsStore;
            result.PsPass = userSettings.PasswordPsStore;
            result.PsCode = userSettings.Code;
            await _orderRepository.Add(order);

            return result;
        }


        public async Task<List<OrderListDto>> OrdersList()
        {
           
            var result = new List<OrderListDto>();

            var orders = (await _orderRepository.GetListQuery()).OrderByDescending(o => o.DateCreate);

            result.AddRange(orders.Select(item => new OrderListDto
            {
                UserChatId = item.TgUserId,
                OrderCode = item.OrderCode,
                Price = item.Price,
                JPrice = item.JPrice,
                Created = item.DateCreate
            }));

            return result;
        }
           


        private static string GenerateCode(Guid guid)
        {
            // Преобразуем GUID в строку и убираем дефисы
            string guidString = guid.ToString("N"); 
            
            string code = guidString.Substring(0, 8).ToUpper();

            return code.Insert(4, "-"); // Преобразуем в формат XXXX-XXXX
        }
    }
}
