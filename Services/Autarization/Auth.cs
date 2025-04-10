using Business.Data.Models;
using Microsoft.AspNetCore.Identity;
using Service.Application.Iterfaces;

namespace Services.Autarization
{
    public class Auth : IAuthService
    {
        private readonly PasswordHasher<Admin> _passwordHasher = new();

        public string Generate(Admin admin)
        {
            return _passwordHasher.HashPassword(admin, admin.Password);
        }

        public bool Verify(Admin admin, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(admin, admin.Password, providedPassword);

            return result == PasswordVerificationResult.Success;
        }

    }
}
