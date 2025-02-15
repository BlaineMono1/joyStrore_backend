namespace Service.Application.Iterfaces
{
    public interface ICalculationService
    {
        Task<decimal> CalcPrice(decimal? priceua, decimal? pricetr, string type, string region);
        Task<decimal> CalcJprice(decimal? price, string region);
        Task<decimal> CalcJplus(decimal price);
    }
}
