
using Business.Data.Enums;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IRepository<Admin> _adminRepository;
        private readonly IRepository<LoyaltyOrder> _loyalityOrderRepository;
        public OrderQuery(IUserRepository<User> userRepository, ICalculationService calculatePrice, IDataFromCookie regionFromCookie, 
                         IRepository<Cart> cartRepository, IProductRepository<Product> productRepository, IRepository<Order> orderRepository, 
                         IRepository<CartItem> cartItemRepository, IRepository<Setting> settingRepository, ILogger<OrderQuery> logger, IRepository<LoyaltyCurrency> loyalitiRepository,
                         IRepository<Admin> adminRepository, IRepository<LoyaltyOrder> loyalityOrderRepository)
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
            _adminRepository = adminRepository;
            _loyalityOrderRepository = loyalityOrderRepository;
        }

        public async Task CreateOrderRub(string PsEmail, string PsPass, string PsCode, string ReciptEmail, bool isSave)
        {
            var (order, totalJPlus) = await ProcessOrder("RUB", PsEmail, PsPass, PsCode, ReciptEmail, isSave);

            var userTgId = _regionFromCookie.GetUserTgID();
            var loyality = (await _loyalitiRepository.GetListQuery())
                .Include(l => l.User).FirstOrDefault(l => l.User.TgUserId == userTgId);
            if (loyality is null) throw new NotFoundException(nameof(LoyaltyCurrency), userTgId);

            loyality.BalanceJoyPlus += totalJPlus;

            await _orderRepository.Add(order);
            await _loyalitiRepository.Update(loyality);

            var cart = (await _cartRepository.GetListQuery()).Include(c => c.User).Include(c => c.CartItems).FirstOrDefault(c => c.User.TgUserId == userTgId);
            if (cart is null) throw new NotFoundException(nameof(Cart), $"for user {userTgId}");

            // Очистка корзины
            foreach (var item in cart.CartItems)
                await _cartItemRepository.HardDelete(item.Guid);

        }

        public async Task CreateOrderJ(string PsEmail, string PsPass, string PsCode, string ReciptEmail, bool isSave)
        {
            var (order, totalJPlus) = await ProcessOrder("J", PsEmail, PsPass, PsCode, ReciptEmail, isSave);
            order.IsJPayment = true;
            var userTgId = _regionFromCookie.GetUserTgID();
            var loyality = (await _loyalitiRepository.GetListQuery())
                .Include(l => l.User).FirstOrDefault(l => l.User.TgUserId == userTgId);
            if (loyality is null) throw new NotFoundException(nameof(LoyaltyCurrency), userTgId);

            if (loyality.BalanceJoy < order.Price)
                throw new BadRequestExeption("Your balance is not sufficient for payment, top it up.");

            var cart = (await _cartRepository.GetListQuery()).Include(c => c.User).Include(c => c.CartItems).FirstOrDefault(c => c.User.TgUserId == userTgId);

            if (cart is null) throw new NotFoundException(nameof(Cart), $"for user {userTgId}");

            loyality.BalanceJoy -= order.Price;
            loyality.BalanceJoyPlus += totalJPlus;

            await _orderRepository.Add(order);
            await _loyalitiRepository.Update(loyality);

            foreach (var item in cart.CartItems)
                await _cartItemRepository.HardDelete(item.Guid);

        }

        public async Task<List<OrderListDto>> WorkerOrders(Guid WorkerId)
        {
           
            var result = new List<OrderListDto>();

            var orders = (await _orderRepository.GetListQuery()).Where(o => o.WorkerId == WorkerId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .Include(o => o.OrderProductItems).ThenInclude(i => i.Product).ToList();
            
            foreach (var order in orders)
            {
                var t = new OrderListDto
                {
                    OrderId = order.Guid,
                    OrderCode = order.OrderCode,
                    UserChatId = order.TgUserId,
                    Items = new List<OrderItemsDto>(),
                    UserInfo = new UserPsInfo
                    {
                       Login = order.PsLogin,
                       Password = order.PsPass,
                       Code = order.Code
                    }
                };

                foreach(var item in order.OrderProductItems)
                {
                    switch (item.Product.Type)
                    {
                        case "Game":
                            var edition = await _productRepository.GetTypeEntity<Edition>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = edition.Name });
                            break;
                        case "AddOn":
                            var addOn = await _productRepository.GetTypeEntity<AddOn>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = addOn.Name });
                            break;
                        default:
                            var sub = await _productRepository.GetTypeEntity<Subscription>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = sub.Name });
                            break;
                    }

                }

                result.Add(t);
            }

            return result;

        }


        public async Task<List<OrderListDto>> NotTakenOreders()
        {
            var result = new List<OrderListDto>();

            var orders = (await _orderRepository.GetListQuery()).Where(o => o.WorkerId == null && o.Status != OrderStatus.Cancelled).Include(o => o.OrderProductItems).ThenInclude(i => i.Product).OrderBy(o => o.DateCreate).ToList();

            foreach (var order in orders)
            {
                var t = new OrderListDto
                {
                    OrderId = order.Guid,
                    OrderCode = order.OrderCode,
                    UserChatId = order.TgUserId,
                    Items = new List<OrderItemsDto>(),
                    UserInfo = new UserPsInfo
                    {
                        Login = order.PsLogin,
                        Password = order.PsPass,
                        Code = order.Code
                    }
                };

                foreach (var item in order.OrderProductItems)
                {
                    switch (item.Product.Type)
                    {
                        case "Game":
                            var edition = await _productRepository.GetTypeEntity<Edition>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = edition.Name });
                            break;
                        case "AddOn":
                            var addOn = await _productRepository.GetTypeEntity<AddOn>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = addOn.Name });
                            break;
                        default:
                            var sub = await _productRepository.GetTypeEntity<Subscription>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = sub.Name });
                            break;
                    }

                }

                result.Add(t);
            }

            return result;
        }


        public async Task TakeOrder(Guid OrderId, Guid WorkerId)
        {
            var oreder = await _orderRepository.GetById(OrderId);

            if (oreder is null) throw new NotFoundException(nameof(Order), OrderId);

            if (oreder.WorkerId != null) throw new BadRequestExeption("Order is alredy taken");

            oreder.WorkerId = WorkerId;
            oreder.Status = OrderStatus.Processing;

            await _orderRepository.Update(oreder);

        }

        public async Task RefuseOrder(Guid OrderId, Guid WorkerId)
        {
            var oreder = await _orderRepository.GetById(OrderId);

            if (oreder is null) throw new NotFoundException(nameof(Order), OrderId);

            if (oreder.WorkerId != WorkerId) throw new BadRequestExeption("This is not your order");

            oreder.WorkerId = null;

            oreder.Status= OrderStatus.Created;

            await _orderRepository.Update(oreder);
        }

        public async Task OrderDone(Guid OrderId, Guid WorkerId)
        {
            var oreder = await _orderRepository.GetById(OrderId);

            if (oreder is null) throw new NotFoundException(nameof(Order), OrderId);

            if (oreder.WorkerId != WorkerId) throw new BadRequestExeption("This is not your order");

            oreder.Status = OrderStatus.Completed;

            await _orderRepository.Update(oreder);
        }

        public async Task CancelOrder(Guid OrderId)
        {
            
            var oreder = await _orderRepository.GetById(OrderId);

            if (oreder is null) throw new NotFoundException(nameof(Order), OrderId);

            var loyality = (await _loyalitiRepository.GetListQuery())
                .Include(l => l.User).FirstOrDefault(l => l.User.TgUserId == oreder.TgUserId);
            if (loyality is null) throw new NotFoundException(nameof(LoyaltyCurrency), oreder.TgUserId);

            if (!oreder.IsJPayment)
            {
                //логика отметы за рубли
            }
            else
            {
                loyality.BalanceJoy += oreder.Price;
                                
            }

            loyality.BalanceJoyPlus -= Math.Min(loyality.BalanceJoyPlus, oreder.TotalJoyPlus); // какая логика у того что joy+ меньше чем в заказе.

            oreder.Status = OrderStatus.Cancelled;

            await _loyalitiRepository.Update(loyality);
            await _orderRepository.Update(oreder);

        }

        public async Task<List<AllOrdersDto>> GetAllOrdersList()
        {
            var result = new List<AllOrdersDto>();

            var orders = (await _orderRepository.GetListQuery()).Include(o => o.OrderProductItems).ThenInclude(i => i.Product).OrderBy(o => o.DateCreate).ToList();

            foreach (var order in orders)
            {
                var t = new AllOrdersDto
                {
                    OrderId = order.Guid,
                    OrderCode = order.OrderCode,
                    UserChatId = order.TgUserId,
                    OrderPrice = order.Price,
                    ManagerLogin = (order.WorkerId is null ? "" : (await _adminRepository.GetById(order.WorkerId.Value)).Login),
                    Status = order.Status.ToString(),
                    Items = new List<OrderItemsDto>(),
                    UserInfo = new UserPsInfo
                    {
                        Login = order.PsLogin,
                        Password = order.PsPass,
                        Code = order.Code
                    }
                };

                foreach (var item in order.OrderProductItems)
                {
                    switch (item.Product.Type)
                    {
                        case "Game":
                            var edition = await _productRepository.GetTypeEntity<Edition>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = edition.Name });
                            break;
                        case "AddOn":
                            var addOn = await _productRepository.GetTypeEntity<AddOn>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = addOn.Name });
                            break;
                        default:
                            var sub = await _productRepository.GetTypeEntity<Subscription>(item.Product);
                            t.Items.Add(new OrderItemsDto { ItemName = sub.Name });
                            break;
                    }

                }

                result.Add(t);
            }

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

        private async Task<(Order order, decimal totalJPlus)> ProcessOrder(string paymentType, string PsEmail, string PsPass, string PsCode, string ReciptEmail, bool isSave)
        {
            var region = _regionFromCookie.GetUserRegion();
            var userTgId = _regionFromCookie.GetUserTgID();

            var order = new Order
            {
                OrderProductItems = new List<OrderProductItem>(),
                Status = Business.Data.Enums.OrderStatus.Created,
                OrderCode = GenerateCode(Guid.NewGuid()),
                TgUserId = userTgId,
                Region = region,
            };

            var user = (await _userRepository.GetListQuery())
                .Include(u => u.Cart).ThenInclude(c => c.CartItems)
                .FirstOrDefault(u => u.TgUserId == userTgId);

            if (user is null) throw new NotFoundException(nameof(User), userTgId);
            if (user.Cart?.CartItems == null || !user.Cart.CartItems.Any())
                throw new BadRequestExeption("Cart is empty");

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
                    Discount = (region == "UAH" ? product.DiscountPercentUa : product.DiscountPercentTr),
                    OrderId = order.Guid,
                    ProductId = product.Guid
                };

                orderItem.JPrice = await _calculatePrice.CalcJprice(orderItem.Price);

                string name = "", cusacode = "";
                switch (product.Type)
                {
                    case "Game":
                        var edition = await _productRepository.GetTypeEntity<Edition>(product);
                        name = edition.Name;
                        cusacode = region == "UAH" ? edition.CusaCodeUa : edition.CusaCodeTr;
                        break;
                    case "AddOn":
                        var addOn = await _productRepository.GetTypeEntity<AddOn>(product);
                        name = addOn.Name;
                        cusacode = region == "UAH" ? addOn.CusaCodeUa : addOn.CusaCodeTr;
                        break;
                    case "Subscription":
                        var sub = await _productRepository.GetTypeEntity<Subscription>(product);
                        name = sub.Name;
                        cusacode = region == "UAH" ? sub.CusaCodeUa : sub.CusaCodeTr;
                        break;
                }

                var orderItemDto = new OrderItemDto
                {
                    Name = name,
                    CusaCode = cusacode,
                    Type = product.Type,
                    Price = paymentType == "J" ? orderItem.JPrice : orderItem.Price
                };

                totalPrice += orderItemDto.Price;
                totalJPlus += await _calculatePrice.CalcJplus(orderItem.JPrice);

                order.OrderProductItems.Add(orderItem);
            }

            order.Price = totalPrice;

            var userSettings = (await _settingRepository.GetListQuery())
                .FirstOrDefault(s => s.UserId == user.Guid && s.Region == region);
            if (userSettings is null) throw new NotFoundException(nameof(Setting), userTgId);
                       
            if(isSave)
            {
                userSettings.EmailPsStore = PsEmail;
                userSettings.PasswordPsStore = PsPass;
                userSettings.Code = PsCode;
                user.Email = ReciptEmail;
                await _settingRepository.Update(userSettings);
                await _userRepository.Update(user);
            }


            order.PsLogin = PsEmail;
            order.PsPass = PsPass;
            order.Code = PsCode;
            order.TotalJoyPlus = totalJPlus;

            return (order, totalJPlus);
        }


        public async Task<List<TransactionsHistoryDto>> TransacionHistoryParams(string ChatId, string CodeOrder)
        {
            var history = await _loyalityOrderRepository.GetListQuery();

            if (!string.IsNullOrEmpty(ChatId)) history = history.Where(h => h.TgUserId == ChatId);

            if (!string.IsNullOrEmpty(CodeOrder)) history = history.Where(h => h.CodeOrder == CodeOrder);


            var result = new List<TransactionsHistoryDto>();

            result.AddRange(history.Select(item => new TransactionsHistoryDto
            {
                TgId = item.TgUserId,
                OrderCode = item.CodeOrder,
                JoyAmount = item.CountProductJoy,
                DateCreate = item.DateCreate,
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
