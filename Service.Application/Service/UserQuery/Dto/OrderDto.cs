namespace Service.Application.Service.UserQuery.Dto
{
    public class OrderDto
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public List<CartItemDto> items { get; set; }

    }
}
