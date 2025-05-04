using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Data;
using Newtonsoft.Json.Linq;
using Service.Application.Exceptions;

namespace Service.Application.Service.AutahQuery
{
    public class AutahQuery
    {
        private readonly IRepository<Admin> _adminRepository;
        private readonly IAuthService _autahService;

        private readonly ILogger<AutahQuery> _logger;

        private readonly string _key;
        private readonly IConfiguration _config;

        public AutahQuery(IRepository<Admin> adminRepository, IAuthService autahService, ILogger<AutahQuery> logger, IConfiguration config)
        {
            _adminRepository = adminRepository;
            _autahService = autahService;
            _logger = logger;

            _config = config;
            _key = _config["JWT_KEY"];
        }

        public async Task<string> LogInByToken(string? Token)
        {
            if (!string.IsNullOrEmpty(Token) && ValidateToken(Token))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(Token);


                var role = token?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var adminId = token?.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                _logger.LogInformation(role);
                _logger.LogInformation(adminId);

                var admin = (await _adminRepository.GetListQuery()).Include(a => a.Role).FirstOrDefault(a => a.Guid.ToString() == adminId);
                if (admin == null)
                {
                    _logger.LogError($"Token {Token} with adminId {adminId} is bad!!!");
                    return String.Empty;
                }
                return (admin.Role.Name == role ? Token : String.Empty);
            }
            
            return String.Empty;
        }
        public async Task<string> LogIn(string Login, string password)
        {        

            var admin = (await _adminRepository.GetListQuery()).FirstOrDefault(a => a.Login == Login);

            if (admin == null)
            {
                throw new ForbiddenExeption("Invalid login or password");
            }

            bool result = _autahService.Verify(admin, password);


            if (!result) throw new ForbiddenExeption("Invalid login or password");


            return await GenerateToken(admin.Guid);           
        }

        public async Task<string> GenerateToken(Guid adminId)
        {
            var admin = (await _adminRepository.GetListQuery()).Include(a => a.Role).FirstOrDefault(a => a.Guid == adminId);

            if (admin == null)
            {
                throw new KeyNotFoundException("admin not found");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, adminId.ToString()),
                new Claim(ClaimTypes.Role, admin.Role.Name)
            };

            int lifetime = 12;

            var token = new JwtSecurityToken
            (
                issuer: _config["JWT_ISSUER"],
                audience: _config["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(lifetime), // Токен живет 12 часов
                signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.UTF8.GetBytes(_key);

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, 
                    ValidIssuer = _config["JWT_ISSUER"],
                    ValidAudience = _config["JWT_AUDIENCE"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                
                var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

                // Если валидатор прошел успешно, то токен действителен
                return true;
            }
            catch (Exception)
            {
                // Ошибка валидации токена
                return false;
            }
        }
    }
}

