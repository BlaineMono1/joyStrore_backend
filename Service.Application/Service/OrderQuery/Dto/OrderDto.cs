namespace Service.Application.Service.OrderQuery.Dto
{
    public class OrderDto
    {
        public string OrderCode { get; set; }
        public string Region { get; set; }
        public string TgUserId { get; set; }
        public List<OrderItemDto> Products { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal UserPaid { get; set; }
        public string Status { get; set; }

        public string PsLogin { get; set; }
        public string PsPass { get; set; }
        public string PsCode {  get; set; }
    }
}
