using ISMSponsor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        public StudentsController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var students = await _context.Students.ToListAsync();
            return View(students);
        }
    }
}