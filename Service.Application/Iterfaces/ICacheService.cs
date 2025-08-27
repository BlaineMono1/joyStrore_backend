namespace Service.Application.Iterfaces
{
    public interface ICacheService
    {
        Task UpdateExchangeRates();
        Task UpdateCashBack();
        Task UpdateMarkUp();
    }
}
