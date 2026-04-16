using ISMSponsor.Data;
using ISMSponsor.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v1/reference")]
    [Authorize(Roles = "admin,admissions,cashier,sponsor")]
    public class ReferenceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReferenceController> _logger;

        public ReferenceController(AppDbContext context, ILogger<ReferenceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get active sponsors for dropdown/lookup selection.
        /// Used by Student Charging Portal to populate sponsor selector.
        /// </summary>
        /// <param name="schoolYearId">Optional: Filter by school year to show only relevant sponsors</param>
        /// <param name="search">Optional: Search by sponsor name or legal name (partial match, case-insensitive)</param>
        /// <param name="limit">Optional: Maximum results to return (default 100, max 500)</param>
        /// <returns>List of active sponsors with reference data</returns>
        [HttpGet("sponsors")]
        [ProducesResponseType(typeof(List<SponsorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<SponsorDto>>> GetSponsors(
            [FromQuery] string? schoolYearId,
            [FromQuery] string? search,
            [FromQuery] int limit = 100)
        {
            try
            {
                // Validate pagination
                if (limit < 1 || limit > 500)
                {
                    return BadRequest(new
                    {
                        error = "Limit must be between 1 and 500",
                        receivedLimit = limit
                    });
                }

                var query = _context.Sponsors
                    .AsNoTracking()
                    .Where(s => s.IsActive);

                // Filter by school year if provided (sponsors with active LoG in that year)
                if (!string.IsNullOrEmpty(schoolYearId))
                {
                    query = query.Where(s => s.LettersOfGuarantee != null && s.LettersOfGuarantee
                        .Any(l => l.SchoolYearId == schoolYearId && l.IsActive));
                }

                // Search by name if provided
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(s =>
                        s.SponsorName.ToLower().Contains(searchLower) ||
                        (s.LegalName != null && s.LegalName.ToLower().Contains(searchLower)));
                }

                var sponsors = await query
                    .OrderBy(s => s.SponsorName)
                    .Take(limit)
                    .Select(s => new SponsorDto
                    {
                        SponsorId = s.SponsorId,
                        SponsorName = s.SponsorName,
                        LegalName = s.LegalName,
                        Tin = s.Tin,
                        IsActive = s.IsActive,
                        CreatedOn = s.CreatedOn,
                        ModifiedOn = s.ModifiedOn
                    })
                    .ToListAsync();

                return Ok(sponsors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsors");
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving sponsors",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get active charge items for dropdown/lookup selection.
        /// Used by Student Charging Portal to populate charge item selector.
        /// </summary>
        /// <param name="categoryId">Optional: Filter by category ID</param>
        /// <param name="gradeLevel">Optional: Filter by grade level (e.g., "Grade 1", "Grade 12")</param>
        /// <param name="search">Optional: Search by item name (partial match, case-insensitive)</param>
        /// <param name="limit">Optional: Maximum results to return (default 200, max 1000)</param>
        /// <returns>List of active charge items with reference data</returns>
        [HttpGet("items")]
        [ProducesResponseType(typeof(List<ItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ItemDto>>> GetItems(
            [FromQuery] string? categoryId,
            [FromQuery] string? gradeLevel,
            [FromQuery] string? search,
            [FromQuery] int limit = 200)
        {
            try
            {
                // Validate pagination
                if (limit < 1 || limit > 1000)
                {
                    return BadRequest(new
                    {
                        error = "Limit must be between 1 and 1000",
                        receivedLimit = limit
                    });
                }

                var query = _context.Items
                    .AsNoTracking()
                    .Include(i => i.Category)
                    .Where(i => i.IsActive);

                // Filter by category if provided
                if (!string.IsNullOrEmpty(categoryId))
                {
                    query = query.Where(i => i.CategoryId == categoryId);
                }

                // Filter by grade level if provided
                if (!string.IsNullOrEmpty(gradeLevel))
                {
                    query = query.Where(i => i.GradeLevel == gradeLevel);
                }

                // Search by name if provided
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(i => i.ItemName.ToLower().Contains(searchLower));
                }

                var items = await query
                    .OrderBy(i => i.ItemName)
                    .Take(limit)
                    .Select(i => new ItemDto
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        CategoryId = i.CategoryId,
                        CategoryName = i.Category != null ? i.Category.CategoryName : string.Empty,
                        Description = i.Description,
                        GradeLevel = i.GradeLevel,
                        Currency = i.Currency,
                        IsActive = i.IsActive
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items");
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving items",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get active charge item categories for dropdown/lookup selection.
        /// Used by Student Charging Portal to populate category selector.
        /// </summary>
        /// <param name="search">Optional: Search by category name (partial match, case-insensitive)</param>
        /// <param name="limit">Optional: Maximum results to return (default 50, max 500)</param>
        /// <returns>List of active item categories with reference data</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<ItemCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ItemCategoryDto>>> GetCategories(
            [FromQuery] string? search,
            [FromQuery] int limit = 50)
        {
            try
            {
                // Validate pagination
                if (limit < 1 || limit > 500)
                {
                    return BadRequest(new
                    {
                        error = "Limit must be between 1 and 500",
                        receivedLimit = limit
                    });
                }

                var query = _context.ItemCategories
                    .AsNoTracking()
                    .Where(c => c.IsActive);

                // Search by name if provided
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(c => c.CategoryName.ToLower().Contains(searchLower));
                }

                var categories = await query
                    .OrderBy(c => c.CategoryName)
                    .Take(limit)
                    .Select(c => new ItemCategoryDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving categories",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get active sponsor-student links.
        /// Returns all active Letter of Guarantee assignments linking sponsors to students.
        /// Used by PowerSchool and Student Charging Portal to maintain sponsor coverage visibility.
        /// </summary>
        /// <param name="sponsorId">Optional: Filter by sponsor ID</param>
        /// <param name="studentId">Optional: Filter by student ID</param>
        /// <param name="schoolYearId">Optional: Filter by school year</param>
        /// <param name="limit">Optional: Maximum results to return (default 500, max 5000)</param>
        /// <returns>List of active sponsor-student links</returns>
        [HttpGet("active-sponsor-links")]
        [ProducesResponseType(typeof(List<ActiveSponsorLinkDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ActiveSponsorLinkDto>>> GetActiveSponsorLinks(
            [FromQuery] string? sponsorId,
            [FromQuery] string? studentId,
            [FromQuery] string? schoolYearId,
            [FromQuery] int limit = 500)
        {
            try
            {
                // Validate pagination
                if (limit < 1 || limit > 5000)
                {
                    return BadRequest(new
                    {
                        error = "Limit must be between 1 and 5000",
                        receivedLimit = limit
                    });
                }

                var query = _context.LogCoverages
                    .AsNoTracking()
                    .Include(l => l.Sponsor)
                    .Include(l => l.Student)
                    .Where(l => l.IsActive);

                // Filter by sponsor if provided
                if (!string.IsNullOrEmpty(sponsorId))
                {
                    query = query.Where(l => l.SponsorId == sponsorId);
                }

                // Filter by student if provided
                if (!string.IsNullOrEmpty(studentId))
                {
                    query = query.Where(l => l.StudentId == studentId);
                }

                // Filter by school year if provided
                if (!string.IsNullOrEmpty(schoolYearId))
                {
                    query = query.Where(l => l.SchoolYearId == schoolYearId);
                }

                var links = await query
                    .OrderBy(l => l.SponsorId)
                    .ThenBy(l => l.StudentId)
                    .Take(limit)
                    .Select(l => new ActiveSponsorLinkDto
                    {
                        StudentId = l.StudentId,
                        StudentName = l.Student != null ? $"{l.Student.FirstName} {l.Student.LastName}" : string.Empty,
                        SponsorId = l.SponsorId,
                        SponsorName = l.Sponsor != null ? l.Sponsor.SponsorName : string.Empty,
                        SchoolYearId = l.SchoolYearId,
                        SchoolYearLabel = l.SchoolYearId,
                        LogId = l.LogId,
                        EffectiveFrom = l.EffectiveFrom ?? DateTime.MinValue,
                        EffectiveTo = l.EffectiveTo,
                        IsActive = l.IsActive
                    })
                    .ToListAsync();

                return Ok(links);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sponsor links");
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving active sponsor links",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
