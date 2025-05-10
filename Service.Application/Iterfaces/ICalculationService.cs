namespace Service.Application.Iterfaces
{
    public interface ICalculationService
    {
        Task<decimal> CalcPrice(decimal? priceua, decimal? pricetr, string type, Guid? id = null);
        Task<decimal> CalcJprice(decimal? price);
        Task<decimal> CalcJplus(decimal price);
    }
}
