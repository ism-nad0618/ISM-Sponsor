using ISMSponsor.Data;
using ISMSponsor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "sponsor")]
public class SponsorReportsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SponsorReportsController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// My Letters of Guarantee - shows all LoGs for the logged-in sponsor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MyLoGs(string?schoolYearId = null, string? status = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrEmpty(user.SponsorId))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var query = _context.LogCoverages
            .Include(l => l.Student)
            .Include(l => l.CoverageRules)
            .Where(l => l.SponsorId == user.SponsorId);

        // Apply filters
        if (!string.IsNullOrEmpty(schoolYearId))
        {
            query = query.Where(l => l.SchoolYearId == schoolYearId);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Active")
                query = query.Where(l => l.IsActive);
            else if (status == "Inactive")
                query = query.Where(l => !l.IsActive);
            else
                query = query.Where(l => l.LogStatus == status);
        }

        var logs = await query
            .OrderByDescending(l => l.CreatedOn)
            .ToListAsync();

        // Get school years for filter dropdown
        ViewBag.SchoolYears = await _context.SchoolYears
            .OrderByDescending(y => y.Name)
            .ToListAsync();

        ViewBag.SelectedSchoolYearId = schoolYearId;
        ViewBag.SelectedStatus = status;
        ViewBag.SponsorId = user.SponsorId;

        // Calculate statistics
        ViewBag.TotalLoGs = logs.Count;
        ViewBag.ActiveLoGs = logs.Count(l => l.IsActive);
        ViewBag.ApprovedLoGs = logs.Count(l => l.LogStatus == "Approved");
        ViewBag.PendingLoGs = logs.Count(l => l.LogStatus == "Pending");

        return View(logs);
    }

    /// <summary>
    /// My Students - shows all students sponsored by the logged-in sponsor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MyStudents(string? schoolYearId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrEmpty(user.SponsorId))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var query = _context.Students
            .Where(s => s.SponsorId == user.SponsorId);

        if (!string.IsNullOrEmpty(schoolYearId))
        {
            query = query.Where(s => s.SchoolYearId == schoolYearId);
        }

        var students = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        // Get school years for filter dropdown
        ViewBag.SchoolYears = await _context.SchoolYears
            .OrderByDescending(y => y.Name)
            .ToListAsync();

        ViewBag.SelectedSchoolYearId = schoolYearId;
        ViewBag.SponsorId = user.SponsorId;

        // Calculate statistics
        ViewBag.TotalStudents = students.Count;
        ViewBag.ActiveStudents = students.Count(s => s.StudentStatus == "active");

        return View(students);
    }
}
