
namespace Service.Application.Service.OrderQuery.Dto
{
    public class UserOrdersListDto
    {
        public string OrderCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<UserOrderItems> Products { get; set; }
    }
}
