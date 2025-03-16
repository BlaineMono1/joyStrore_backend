namespace Service.Application.Iterfaces
{
    public interface ICalculationService
    {
        Task<decimal> CalcPrice(decimal? priceua, decimal? pricetr, string type);
        Task<decimal> CalcJprice(decimal? price);
        Task<decimal> CalcJplus(decimal price);
    }
}
