
namespace Service.Application.Service.UserQuery.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
        public string Platform { get; set; }
    }
}
