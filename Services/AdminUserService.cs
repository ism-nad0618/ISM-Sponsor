using ISMSponsor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class AdminUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public Task<List<ApplicationUser>> GetAllAsync() => _userManager.Users.ToListAsync();

        public Task<List<IdentityRole>> GetRolesAsync() => _roleManager.Roles.ToListAsync();

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, string role)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            return result;
        }

        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
            => _userManager.AddToRoleAsync(user, role);
    }
}