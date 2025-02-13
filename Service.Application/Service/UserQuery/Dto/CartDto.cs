namespace Service.Application.Service.UserQuery.Dto
{
    public class CartDto
    {
        public List<CartItemDto> items { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
        public string PayEmail { get; set; }
    }
}
