
namespace Service.Application.Service.OrderQuery.Dto
{
    public class OrderListDto
    {
        public Guid OrderId { get; set; }
        public string UserChatId { get; set; }
        public string OrderCode { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public DateTime Created { get; set; }
    }
}
