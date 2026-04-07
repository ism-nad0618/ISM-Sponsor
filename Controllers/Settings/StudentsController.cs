using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        public StudentsController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? search, string? schoolYear, string? status, string? sortBy, string? sortOrder)
        {
            var query = _context.Students.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => 
                    s.StudentId.Contains(search) ||
                    s.FirstName.Contains(search) ||
                    s.LastName.Contains(search) ||
                    s.SponsorId.Contains(search));
                ViewBag.Search = search;
            }

            // Apply school year filter
            if (!string.IsNullOrWhiteSpace(schoolYear))
            {
                query = query.Where(s => s.SchoolYearId == schoolYear);
                ViewBag.SchoolYear = schoolYear;
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.StudentStatus == status);
                ViewBag.Status = status;
            }

            // Apply sorting
            sortBy = sortBy ?? "LastName";
            sortOrder = sortOrder ?? "asc";
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            query = sortBy switch
            {
                "StudentId" => sortOrder == "asc" ? query.OrderBy(s => s.StudentId) : query.OrderByDescending(s => s.StudentId),
                "FirstName" => sortOrder == "asc" ? query.OrderBy(s => s.FirstName) : query.OrderByDescending(s => s.FirstName),
                "GradeLevel" => sortOrder == "asc" ? query.OrderBy(s => s.GradeLevel) : query.OrderByDescending(s => s.GradeLevel),
                "SchoolYearId" => sortOrder == "asc" ? query.OrderBy(s => s.SchoolYearId) : query.OrderByDescending(s => s.SchoolYearId),
                "Status" => sortOrder == "asc" ? query.OrderBy(s => s.StudentStatus) : query.OrderByDescending(s => s.StudentStatus),
                _ => sortOrder == "asc" ? query.OrderBy(s => s.LastName) : query.OrderByDescending(s => s.LastName)
            };

            var students = await query.ToListAsync();

            // Get distinct school years and statuses for filter dropdowns
            ViewBag.SchoolYears = await _context.Students.Select(s => s.SchoolYearId).Distinct().OrderByDescending(y => y).ToListAsync();
            ViewBag.Statuses = await _context.Students.Select(s => s.StudentStatus).Distinct().OrderBy(st => st).ToListAsync();
            ViewBag.Sponsors = await _context.Sponsors.OrderBy(sp => sp.SponsorName).ToListAsync();
            ViewBag.AllSchoolYears = await _context.SchoolYears.OrderByDescending(y => y.SchoolYearId).ToListAsync();

            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            // Check if student already exists
            var existing = await _context.Students
                .FirstOrDefaultAsync(s => s.SchoolYearId == student.SchoolYearId && s.StudentId == student.StudentId);
            
            if (existing != null)
            {
                return Json(new { success = false, message = "Student already exists for this school year" });
            }

            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to create student: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditInline([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var existing = await _context.Students
                .FirstOrDefaultAsync(s => s.SchoolYearId == student.SchoolYearId && s.StudentId == student.StudentId);
            
            if (existing == null)
            {
                return Json(new { success = false, message = "Student not found" });
            }

            existing.FirstName = student.FirstName;
            existing.LastName = student.LastName;
            existing.GradeLevel = student.GradeLevel;
            existing.SponsorId = student.SponsorId;
            existing.StudentStatus = student.StudentStatus;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student updated successfully" });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to update student" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Only CSV files are supported" });
            }

            try
            {
                var students = new List<Student>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    students = csv.GetRecords<Student>().ToList();
                }

                if (students.Count == 0)
                {
                    return Json(new { success = false, message = "No valid records found in CSV" });
                }

                var validationResults = new List<object>();
                int willAdd = 0, willUpdate = 0, hasErrors = 0;

                foreach (var student in students)
                {
                    var itemErrors = new List<string>();
                    string action = "Skip";
                    
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(student.SchoolYearId))
                        itemErrors.Add("Missing SchoolYearId");
                    if (string.IsNullOrWhiteSpace(student.StudentId))
                        itemErrors.Add("Missing StudentId");
                    if (string.IsNullOrWhiteSpace(student.FirstName))
                        itemErrors.Add("Missing FirstName");
                    if (string.IsNullOrWhiteSpace(student.LastName))
                        itemErrors.Add("Missing LastName");

                    // Check if student exists
                    var existing = await _context.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.SchoolYearId == student.SchoolYearId && s.StudentId == student.StudentId);
                    
                    if (itemErrors.Count == 0)
                    {
                        if (existing == null)
                        {
                            action = "Add";
                            willAdd++;
                        }
                        else
                        {
                            action = "Update";
                            willUpdate++;
                        }
                    }
                    else
                    {
                        hasErrors++;
                    }

                    validationResults.Add(new
                    {
                        schoolYearId = student.SchoolYearId,
                        studentId = student.StudentId,
                        firstName = student.FirstName,
                        lastName = student.LastName,
                        gradeLevel = student.GradeLevel,
                        sponsorId = student.SponsorId,
                        studentStatus = student.StudentStatus,
                        action,
                        errors = itemErrors,
                        hasError = itemErrors.Count > 0
                    });
                }

                return Json(new
                {
                    success = true,
                    items = validationResults,
                    summary = new
                    {
                        total = students.Count,
                        willAdd,
                        willUpdate,
                        hasErrors
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Validation failed: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Only CSV files are supported" });
            }

            try
            {
                var students = new List<Student>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    students = csv.GetRecords<Student>().ToList();
                }

                if (students.Count == 0)
                {
                    return Json(new { success = false, message = "No valid records found in CSV" });
                }

                int added = 0, updated = 0, skipped = 0;

                foreach (var student in students)
                {
                    var existing = await _context.Students
                        .FirstOrDefaultAsync(s => s.SchoolYearId == student.SchoolYearId && s.StudentId == student.StudentId);
                    
                    if (existing == null)
                    {
                        _context.Students.Add(student);
                        added++;
                    }
                    else
                    {
                        existing.FirstName = student.FirstName;
                        existing.LastName = student.LastName;
                        existing.GradeLevel = student.GradeLevel;
                        existing.SponsorId = student.SponsorId;
                        existing.StudentStatus = student.StudentStatus;
                        updated++;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Import completed: {added} added, {updated} updated, {skipped} skipped",
                    added,
                    updated,
                    skipped
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Import failed: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportTemplate()
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write header
                csv.WriteField("SchoolYearId");
                csv.WriteField("StudentId");
                csv.WriteField("FirstName");
                csv.WriteField("LastName");
                csv.WriteField("GradeLevel");
                csv.WriteField("SponsorId");
                csv.WriteField("StudentStatus");
                csv.NextRecord();

                // Write sample row
                csv.WriteField("25-26");
                csv.WriteField("STU001");
                csv.WriteField("John");
                csv.WriteField("Doe");
                csv.WriteField("9");
                csv.WriteField("SP001");
                csv.WriteField("Active");
                csv.NextRecord();
            }

            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "students_template.csv");
        }
    }
}
