namespace Service.Application.Service.TransactionQuery.Dto
{
    public class JoyesDonsDto
    {
        public List<decimal> Joy { get; set; } =
            new List<decimal> { 15, 500, 1000, 2500, 5000, 10000 };
        public List<decimal> JoyPlus { get; set; } =
            new List<decimal> { 100, 500, 1000, 2500, 5000, 10000 };
    }
}
