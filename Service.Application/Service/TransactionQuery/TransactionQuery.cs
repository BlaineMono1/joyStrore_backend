using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Service.Application.Service.TransactionQuery
{
    public class TransactionQuery
    {
        private readonly IRepository<LoyaltyCurrency> _joyBalRepository;
        private readonly ILogger<TransactionQuery> _logger;

        public TransactionQuery(IRepository<LoyaltyCurrency> joyBalRepository, ILogger<TransactionQuery> logger)
        {
            _joyBalRepository = joyBalRepository;
            _logger = logger;
        }

        public async Task IncUserJoyBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s => s.User.TgUserId == tgId);

            if (setting == null) throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0) throw new BadRequestExeption("Invalid tockens amount");

            setting.BalanceJoy += amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task DecUserJoyBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s => s.User.TgUserId == tgId);

            if (setting == null) throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0) throw new BadRequestExeption("Invalid tockens amount");

            if (amount > setting.BalanceJoy) throw new BadRequestExeption($"User joy balance lower then {amount}");

            setting.BalanceJoy -= amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task IncUserJoyPlusBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s => s.User.TgUserId == tgId);

            if (setting == null) throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0) throw new BadRequestExeption("Invalid tockens amount");

            setting.BalanceJoyPlus += amount;

            await _joyBalRepository.Update(setting);
        }

        public async Task DecUserJoyPlusBal(string tgId, decimal amount)
        {
            var setting = (await _joyBalRepository.GetListQuery()).FirstOrDefault(s => s.User.TgUserId == tgId);

            if (setting == null) throw new NotFoundException(nameof(LoyaltyCurrency), tgId);

            if (amount <= 0) throw new Exception("Invalid tockens amount");

            if (amount > setting.BalanceJoyPlus) throw new BadRequestExeption($"User joy+ balance lower then {amount}");
            setting.BalanceJoyPlus -= amount;

            await _joyBalRepository.Update(setting);
        }


    }
}
