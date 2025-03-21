
namespace Service.Application.Service.TransactionQuery.Dto
{
    public class JoyesDonsDto
    {
        public List<int> Joy { get; set; } = new List<int> { 500, 1000, 2500, 5000, 10000};
        public List<int> JoyPlus { get; set; } = new List<int> {100, 500, 1000, 2500, 5000, 10000 };
    }
}
