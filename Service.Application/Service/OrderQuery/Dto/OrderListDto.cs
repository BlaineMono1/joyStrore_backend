using Business.Data.Enums;

namespace Service.Application.Service.OrderQuery.Dto
{
    public class OrderListDto
    {
        public Guid OrderId { get; set; }
        public string UserChatId { get; set; }
        public string OrderCode { get; set; }
        public UserPsInfo UserInfo { get; set; }
        public List<OrderItemsDto> Items { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
    }
}
