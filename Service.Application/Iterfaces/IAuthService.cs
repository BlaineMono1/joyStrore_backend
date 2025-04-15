using Business.Data.Models;

namespace Service.Application.Iterfaces
{
    public interface IAuthService
    {
        string Generate(Admin admin);
        bool Verify(Admin admin, string providedPassword);
    }
}
