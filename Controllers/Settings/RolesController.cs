using ISMSponsor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class RolesController : Controller
    {
        private readonly AdminUserService _adminService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(AdminUserService adminService, RoleManager<IdentityRole> roleManager)
        {
            _adminService = adminService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _adminService.GetRolesAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> EditInline([FromBody] RoleUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found" });
            }

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Role updated successfully" });
            }

            return Json(new { success = false, message = "Failed to update role" });
        }
    }

    public class RoleUpdateModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}