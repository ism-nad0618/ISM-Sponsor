using ISMSponsor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class ItemsController : Controller
    {
        private readonly AppDbContext _context;
        public ItemsController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.ToListAsync();
            return View(items);
        }
    }
}