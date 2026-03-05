using ISMSponsor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class RolesController : Controller
    {
        private readonly AdminUserService _adminService;

        public RolesController(AdminUserService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _adminService.GetRolesAsync();
            return View(roles);
        }
    }
}