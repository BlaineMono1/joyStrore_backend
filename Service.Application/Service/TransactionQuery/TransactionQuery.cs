using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Iterfaces;
using Service.Application.Service.TransactionQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Service.Application.Service.TransactionQuery
{
    public class TransactionQuery
    {
        private readonly IRepository<LoyaltyCurrency> _joyBalRepository;
        private readonly IRepository<LoyaltyOrder> _orderRepository;
        private readonly ILogger<TransactionQuery> _logger;
        private readonly IDataFromCookie _dataFromCookie;

        public TransactionQuery(
            IRepository<LoyaltyCurrency> joyBalRepository,
            ILogger<TransactionQuery> logger,
            IDataFromCookie dataFromCookie,
            IRepository<LoyaltyOrder> orderRepository
        )
        {
            _joyBalRepository = joyBalRepository;
            _logger = logger;
            _dataFromCookie = dataFromCookie;
            _orderRepository = orderRepository;
        }

        public async Task IncUserJoyBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );

            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0)
                throw new BadRequestExeption("Invalid tockens amount");

            setting.BalanceJoy += amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task DecUserJoyBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );

            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0)
                throw new BadRequestExeption("Invalid tockens amount");

            if (amount > setting.BalanceJoy)
                throw new BadRequestExeption($"User joy balance lower then {amount}");

            setting.BalanceJoy -= amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task IncUserJoyPlusBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );

            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0)
                throw new BadRequestExeption("Invalid tockens amount");

            setting.BalanceJoyPlus += amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task DecUserJoyPlusBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );

            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0)
                throw new Exception("Invalid tockens amount");

            if (amount > setting.BalanceJoyPlus)
                throw new BadRequestExeption($"User joy+ balance lower then {amount}");
            setting.BalanceJoyPlus -= amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task<decimal> UserJoyBalAfterrRplenishment(decimal Joy)
        {
            var tgId = _dataFromCookie.GetUserTgID();

            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );

            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            return setting.BalanceJoy + Joy;
        }

        public async Task BuyJoyRub(decimal JoyAmount)
        {
            var tgId = _dataFromCookie.GetUserTgID();

            var amount = new JoyesDonsDto();

            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );
            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (!amount.Joy.Contains(JoyAmount))
                throw new BadRequestExeption("Invalid amount of joy");

            var order = new LoyaltyOrder
            {
                TgUserId = tgId,
                CountProductJoy = JoyAmount,
                AmountPayment = JoyAmount,
                ByJoyPlus = false,
            };

            setting.BalanceJoy += JoyAmount;
            order.CodeOrder = GenerateCode(order.Guid);

            await _joyBalRepository.Update(setting);
            await _orderRepository.Add(order);
        }

        public async Task BuyJoy(decimal JoyAmount)
        {
            var tgId = _dataFromCookie.GetUserTgID();

            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s =>
                s.User.TgUserId == tgId
            );

            if (setting == null)
                throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            var amount = new JoyesDonsDto();

            if (!amount.JoyPlus.Contains(JoyAmount))
                throw new BadRequestExeption("Invalid amount of joy");

            if (setting.BalanceJoyPlus < JoyAmount)
                throw new BadRequestExeption("Not enough joy+");

            setting.BalanceJoyPlus -= JoyAmount;
            setting.BalanceJoy += JoyAmount;

            var order = new LoyaltyOrder
            {
                TgUserId = tgId,
                CountProductJoy = JoyAmount,
                AmountPayment = JoyAmount,
                ByJoyPlus = true,
            };

            order.CodeOrder = GenerateCode(order.Guid);

            await _joyBalRepository.Update(setting);
            await _orderRepository.Add(order);
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
