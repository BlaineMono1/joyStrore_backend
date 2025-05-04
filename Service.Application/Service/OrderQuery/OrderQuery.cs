
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Iterfaces;
using Service.Application.Service.OrderQuery.Dto;
using System.Xml.Linq;
using static Service.Application.Exceptions.NotFoundExeption;

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
        private readonly IRepository<LoyaltyCurrency> _loyalitiRepository;
        private readonly ICalculationService _calculatePrice;
        private readonly IDataFromCookie _regionFromCookie;
        private readonly ILogger<OrderQuery> _logger;
        public OrderQuery(IUserRepository<User> userRepository, ICalculationService calculatePrice, IDataFromCookie regionFromCookie, 
                         IRepository<Cart> cartRepository, IProductRepository<Product> productRepository, IRepository<Order> orderRepository, 
                         IRepository<CartItem> cartItemRepository, IRepository<Setting> settingRepository, ILogger<OrderQuery> logger, IRepository<LoyaltyCurrency> loyalitiRepository)
        {
            _userRepository = userRepository;
            _calculatePrice = calculatePrice;
            _regionFromCookie = regionFromCookie;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _settingRepository = settingRepository;
            _logger = logger;
            _loyalitiRepository = loyalitiRepository;
        }

        public async Task<OrdersDto> CreateOrderRub() //Создание ордера в бд
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

            var user = (await _userRepository.GetListQuery()).FirstOrDefault(u => u.TgUserId == userTgId);

            if (user is null) throw new NotFoundException(nameof(User), userTgId);

            var cart = (await _cartRepository.GetListQuery()).Include(c => c.CartItems).First(c => c.UserId == user.Guid);

            if(cart.CartItems is null || cart.CartItems.Count == 0)
            {
                throw new BadRequestExeption("Cart is empty");
            }

            decimal totalPrice = 0, totalJPlus = 0;
            foreach (var item in cart.CartItems)
            {
                var product = await _productRepository.GetById(item.ProductId);

                if (product is null)
                {
                    _logger.LogError($"Not found product with GUID {item.ProductId}");
                    continue;
                }

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

                
                orderItemDto.Price = orderItem.Price;
                result.Products.Add(orderItemDto);

                totalPrice += orderItem.Price;
                totalJPlus += await _calculatePrice.CalcJplus(orderItem.JPrice);

                order.OrderProductItems.Add(orderItem);
                await _cartItemRepository.HardDelete(item.Guid);

            }

            order.Price = totalPrice;
            
            result.OrderCode = order.OrderCode;
            result.TotalPrice = order.Price;
            result.UserPaid = result.TotalPrice; //Заглушка
            result.Status = order.Status.ToString();

            var userSettings = (await _settingRepository.GetListQuery()).FirstOrDefault(s => s.UserId == user.Guid && s.Region == region);
            if (userSettings is null) throw new NotFoundException(nameof(Setting), userTgId);
            result.PsLogin = userSettings.EmailPsStore;
            result.PsPass = userSettings.PasswordPsStore;
            result.PsCode = userSettings.Code;
            await _orderRepository.Add(order);

            var loyality = (await _loyalitiRepository.GetListQuery()).Include(l => l.User).FirstOrDefault(l => l.User.TgUserId == userTgId);
            if (loyality is null) throw new NotFoundException(nameof(LoyaltyCurrency), userTgId);
            loyality.BalanceJoyPlus += totalJPlus;
            await _loyalitiRepository.Update(loyality);
            return result;
        }

        public async Task<OrdersDto> CreateOrderJ()
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

            var user = (await _userRepository.GetListQuery()).AsNoTracking().Include(u => u.LoyaltyCurrency)
                .Include(u => u.Cart).ThenInclude(c=>c.CartItems)
                .FirstOrDefault(u => u.TgUserId == userTgId);

            if (user is null) throw new NotFoundException(nameof(User), userTgId);

            if (user.Cart.CartItems is null || user.Cart.CartItems.Count == 0) throw new BadRequestExeption("Cart is empty");

            decimal totalPrice = 0, totalJPlus = 0;
            foreach (var item in user.Cart.CartItems)
            {
                var product = await _productRepository.GetById(item.ProductId);

                if (product is null)
                {
                    _logger.LogError($"Not found product with GUID {item.ProductId}");
                    continue;
                }

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
                    case "AddOn":
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


                orderItemDto.Price = orderItem.JPrice;
                result.Products.Add(orderItemDto);

                order.OrderProductItems.Add(orderItem);

                totalPrice += orderItem.JPrice;
                totalJPlus += await _calculatePrice.CalcJplus(orderItem.JPrice);
            }
            order.Price = totalPrice;
            if(user.LoyaltyCurrency.BalanceJoy >= totalPrice)
            {
               
                foreach (var item in user.Cart.CartItems) await _cartItemRepository.HardDelete(item.Guid);

                result.OrderCode = order.OrderCode;
                result.TotalPrice = order.Price;
                result.UserPaid = result.TotalPrice;
                result.Status = order.Status.ToString();

                var userSettings = (await _settingRepository.GetListQuery()).FirstOrDefault(s => s.UserId == user.Guid && s.Region == region);
                if (userSettings is null) throw new NotFoundException(nameof(Setting), userTgId);

                result.PsLogin = userSettings.EmailPsStore;
                result.PsPass = userSettings.PasswordPsStore;
                result.PsCode = userSettings.Code;
                await _orderRepository.Add(order);

                var loyality = (await _loyalitiRepository.GetListQuery()).Include(l => l.User).FirstOrDefault(l => l.User.TgUserId == userTgId);
                if (loyality is null) throw new NotFoundException(nameof(LoyaltyCurrency), userTgId);
                loyality.BalanceJoy -= totalPrice;
                loyality.BalanceJoyPlus += totalJPlus;
                await _loyalitiRepository.Update(loyality);
                               

                return result;
            }
            else
            {
                throw new BadRequestExeption("Your balance is not sufficient for payment, top it up.");
            }

        }

        public async Task<List<OrderListDto>> OrdersList()
        {
           
            var result = new List<OrderListDto>();

            var orders = (await _orderRepository.GetListQuery()).OrderByDescending(o => o.DateCreate);

            result.AddRange(orders.Select(item => new OrderListDto
            {
                OrderId = item.Guid,
                UserChatId = item.TgUserId,
                OrderCode = item.OrderCode,
                Price = item.Price,
                JPrice = item.JPrice,
                Created = item.DateCreate
            }));

            return result;
        }
           
        public async Task<List<UserOrdersListDto>> GetUserOrldersList()
        {
            var result = new List<UserOrdersListDto>();
            var userTgId = _regionFromCookie.GetUserTgID();

            var userOrders = (await _orderRepository.GetListQuery()).Include(o => o.OrderProductItems).ThenInclude(i => i.Product).Where(o => o.TgUserId == userTgId).ToList();

            foreach (var o in userOrders)
            {
                var item = new UserOrdersListDto
                {
                    OrderCode = o.OrderCode,
                    CreatedDate = o.DateCreate,
                    Products = new List<UserOrderItems>()
                };

                foreach(var i in o.OrderProductItems)
                {
                    string name = "", editionType = "", percent = "", platform = "", url = "";

                    switch (i.Product.Type)
                    {
                        case "Game":
                            var edition = await _productRepository.GetTypeEntity<Edition>(i.Product);
                            name = edition.Name;
                            editionType = edition.EditionType;
                            platform = edition.Platform;
                            url = edition.Image;
                            break;
                        case "AddOn":
                            var addOn = await _productRepository.GetTypeEntity<AddOn>(i.Product);
                            name = addOn.Name;
                            platform = addOn.Platform;
                            url = addOn.Image;
                            break;
                        default:
                            var sub = await _productRepository.GetTypeEntity<Subscription>(i.Product);
                            name = sub.Name;
                            platform = sub.Platform;
                            url = sub.Image;
                            break;
                    }


                    item.Products.Add(new UserOrderItems
                    {
                        ProdcuctId = i.ProductId,
                        Name = name,
                        Url = url,
                        EditionType = editionType,
                        Price = i.Price,
                        JPrice = i.JPrice,
                        Percent = i.Discount,
                        Platform = platform

                    });
                }
                result.Add(item);
            }

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
