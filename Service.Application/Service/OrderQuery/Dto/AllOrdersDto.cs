namespace Service.Application.Service.OrderQuery.Dto
{
    public class AllOrdersDto
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; }
        public decimal OrderPrice { get; set; }
        public string ManagerLogin { get; set; }
        public string Status { get; set; }
        public string UserChatId { get; set; }
        public string NewAccount { get; set; }

        public UserPsInfo UserInfo { get; set; }
        public List<OrderItemsDto> Items { get; set; }
    }
}
