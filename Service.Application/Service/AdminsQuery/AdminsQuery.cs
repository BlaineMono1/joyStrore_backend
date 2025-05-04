using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Service.AdminsQuery.Dto;
using Microsoft.AspNetCore.Identity;
using Service.Application.Iterfaces;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Service.Application.Service.AdminsQuery
{
    public class AdminsQuery
    {
        private readonly IRepository<Admin> _adminRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<AdminsQuery> _logger;

        public AdminsQuery(IRepository<Admin> adminRepository, IRepository<Role> roleRepository, ILogger<AdminsQuery> logger, IAuthService authService)
        {
            _adminRepository = adminRepository;
            _roleRepository = roleRepository;
            _logger = logger;
            _authService = authService;
        }

        public async Task<List<AdminListDto>> AdminsList()
        {
            var admins = await _adminRepository.GetAllList();

            var result = admins.Select(item => new AdminListDto
            {
                AdminId = item.Guid,
                Login = item.Login,
            }).ToList();

            return result;
        }

        public async Task<List<RolesList>> RolesList()
        {
            var roles = await _roleRepository.GetAllList();

            var result = roles.Select(item => new RolesList
            {
                RoleId = item.Guid,
                Name = item.Name,
            }).ToList();

            return result;
        }

        public async Task CreateAdmin(string Login, string Password, Guid RoleID)
        {
            var role = await _roleRepository.GetById(RoleID);

            if (role is null) throw new NotFoundException(nameof(Role), RoleID);

            var admin = new Admin { Login = Login, Password = Password, RoleId = RoleID };

            admin.Password = _authService.Generate(admin);

            await _adminRepository.Add(admin);
        }

        public async Task<string> ShowAdminRole(Guid AdminId)
        {
            var admin = await _adminRepository.GetById(AdminId);

            if (admin is null) throw new NotFoundException(nameof(Admin), AdminId);

            var role = await _roleRepository.GetById(admin.RoleId);
            
            return (role is null ? "" : role.Name);
        }

        public async Task UpdateAdmin(Guid AdminId, Guid RoleId)
        {
            var admin = await _adminRepository.GetById(AdminId);
            var role = await _roleRepository.GetById(RoleId);

            if (admin is null) throw new NotFoundException(nameof(Admin), AdminId);
            if (role is null) throw new NotFoundException(nameof(Role), RoleId);

            admin.RoleId = RoleId;

            await _adminRepository.Update(admin);
        }

        public async Task DeleteAdmin(Guid AdminId)
        {
            await _adminRepository.HardDelete(AdminId);
        }      
    }
}
