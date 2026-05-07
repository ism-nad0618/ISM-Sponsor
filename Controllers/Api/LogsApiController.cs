using ISMSponsor.Data;
using ISMSponsor.Models.API;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    /// <summary>
    /// REST API for Letter of Guarantee (LoG) management.
    /// Provides endpoints for creating and managing sponsor coverage agreements.
    /// </summary>
    [ApiController]
    [Route("api/v1/logs")]
    [AllowAnonymous] // Demo: Allow Swagger testing without authentication
    // [Authorize(Roles = "admin,admissions,cashier")] // TODO: Re-enable for production
    [Produces("application/json")]
    public class LogsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LogsApiController> _logger;

        public LogsApiController(AppDbContext context, ILogger<LogsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all Letters of Guarantee (LoG) with optional filtering
        /// </summary>
        /// <param name="schoolYearId">Filter by school year</param>
        /// <param name="sponsorId">Filter by sponsor ID</param>
        /// <param name="studentId">Filter by student ID</param>
        /// <param name="activeOnly">Filter to active LoGs only (default: true)</param>
        /// <returns>List of Letters of Guarantee</returns>
        /// <remarks>
        /// Sample request (using demo data):
        ///
        ///     GET /api/v1/logs?schoolYearId=25-26&amp;sponsorId=DEMO-SP001&amp;activeOnly=true
        ///
        /// Returns all active LoGs for Global Tech Corporation in 2025-2026 school year
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(List<LogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<LogDto>>> GetLogs(
            [FromQuery] string? schoolYearId = null,
            [FromQuery] string? sponsorId = null,
            [FromQuery] string? studentId = null,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var query = _context.LogCoverages
                    .Include(l => l.CoverageRules)
                    .AsNoTracking();

                if (activeOnly)
                {
                    query = query.Where(l => l.IsActive);
                }

                if (!string.IsNullOrWhiteSpace(schoolYearId))
                {
                    query = query.Where(l => l.SchoolYearId == schoolYearId);
                }

                if (!string.IsNullOrWhiteSpace(sponsorId))
                {
                    query = query.Where(l => l.SponsorId == sponsorId);
                }

                if (!string.IsNullOrWhiteSpace(studentId))
                {
                    query = query.Where(l => l.StudentId == studentId);
                }

                var logs = await query
                    .OrderByDescending(l => l.CreatedOn)
                    .Select(l => new LogDto
                    {
                        LogId = l.LogId,
                        SchoolYearId = l.SchoolYearId,
                        StudentId = l.StudentId,
                        SponsorId = l.SponsorId,
                        LogStatus = l.LogStatus,
                        IsActive = l.IsActive,
                        EffectiveFrom = l.EffectiveFrom,
                        EffectiveTo = l.EffectiveTo,
                        CreatedOn = l.CreatedOn,
                        ModifiedOn = l.ModifiedOn,
                        Items = l.CoverageRules.Select(r => new LogItemDto
                        {
                            RuleId = r.RuleId,
                            CoverageTarget = r.CoverageTarget,
                            ItemId = r.ItemId,
                            CategoryId = r.CategoryId,
                            CoverageType = r.CoverageType,
                            CoveragePercentage = r.CoveragePercentage,
                            CoverageFixedAmount = r.CoverageFixedAmount,
                            CapAmount = r.CapAmount
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Letters of Guarantee");
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving Letters of Guarantee",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Create a new Letter of Guarantee (LoG)
        /// </summary>
        /// <param name="request">LoG creation details</param>
        /// <returns>Created LoG with ID</returns>
        /// <remarks>
        /// Sample request (using demo data):
        ///
        ///     POST /api/v1/logs
        ///     {
        ///       "schoolYearId": "25-26",
        ///       "studentId": "DEMO-ST001",
        ///       "sponsorId": "DEMO-SP001",
        ///       "effectiveFrom": "2025-08-15",
        ///       "effectiveTo": "2026-06-15",
        ///       "notes": "Full coverage LoG for Emma Wilson"
        ///     }
        ///
        /// Creates draft LoG for Emma Wilson (Global Tech Corporation) for school year 2025-2026
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(LogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LogDto>> CreateLog([FromBody] CreateLogRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Validate references exist
                var studentExists = await _context.Students.AnyAsync(s => s.StudentId == request.StudentId);
                if (!studentExists)
                {
                    return BadRequest(new { error = $"Student '{request.StudentId}' not found" });
                }

                var sponsorExists = await _context.Sponsors.AnyAsync(s => s.SponsorId == request.SponsorId && s.IsActive);
                if (!sponsorExists)
                {
                    return BadRequest(new { error = $"Sponsor '{request.SponsorId}' not found or inactive" });
                }

                var schoolYearExists = await _context.SchoolYears.AnyAsync(sy => sy.SchoolYearId == request.SchoolYearId);
                if (!schoolYearExists)
                {
                    return BadRequest(new { error = $"School year '{request.SchoolYearId}' not found" });
                }

                var newLog = new LogCoverage
                {
                    SchoolYearId = request.SchoolYearId,
                    StudentId = request.StudentId,
                    SponsorId = request.SponsorId,
                    LogStatus = "Draft",
                    IsActive = false, // Requires activation
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveTo = request.EffectiveTo,
                    Notes = request.Notes,
                    CreatedOn = DateTime.UtcNow
                };

                _context.LogCoverages.Add(newLog);
                await _context.SaveChangesAsync();

                var result = new LogDto
                {
                    LogId = newLog.LogId,
                    SchoolYearId = newLog.SchoolYearId,
                    StudentId = newLog.StudentId,
                    SponsorId = newLog.SponsorId,
                    LogStatus = newLog.LogStatus,
                    IsActive = newLog.IsActive,
                    EffectiveFrom = newLog.EffectiveFrom,
                    EffectiveTo = newLog.EffectiveTo,
                    CreatedOn = newLog.CreatedOn,
                    ModifiedOn = newLog.ModifiedOn,
                    Items = new List<LogItemDto>()
                };

                return CreatedAtAction(nameof(GetLogs), new { logId = newLog.LogId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Letter of Guarantee");
                return StatusCode(500, new
                {
                    error = "An error occurred creating the Letter of Guarantee",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Add coverage items to an existing Letter of Guarantee
        /// </summary>
        /// <param name="logRecordId">LoG ID</param>
        /// <param name="request">Coverage item details</param>
        /// <returns>Updated LoG with new item</returns>
        /// <remarks>
        /// Sample request (full coverage for specific item):
        ///
        ///     POST /api/v1/logs/123/items
        ///     {
        ///       "coverageTarget": "Item",
        ///       "itemId": "TUITION-ELEM",
        ///       "coverageType": "Full"
        ///     }
        ///
        /// Sample request (80% coverage for category with cap):
        ///
        ///     POST /api/v1/logs/123/items
        ///     {
        ///       "coverageTarget": "Category",
        ///       "categoryId": "SUPPLIES",
        ///       "coverageType": "Percentage",
        ///       "coveragePercentage": 80.0,
        ///       "capAmount": 5000.00
        ///     }
        ///
        /// </remarks>
        [HttpPost("{logRecordId}/items")]
        [ProducesResponseType(typeof(LogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LogDto>> AddLogItem(int logRecordId, [FromBody] AddLogItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var log = await _context.LogCoverages
                    .Include(l => l.CoverageRules)
                    .FirstOrDefaultAsync(l => l.LogId == logRecordId);

                if (log == null)
                {
                    return NotFound(new { error = $"Letter of Guarantee {logRecordId} not found" });
                }

                // Validate coverage target exists
                if (request.CoverageTarget == "Item" && !string.IsNullOrWhiteSpace(request.ItemId))
                {
                    var itemExists = await _context.Items.AnyAsync(i => i.ItemId == request.ItemId);
                    if (!itemExists)
                    {
                        return BadRequest(new { error = $"Item '{request.ItemId}' not found" });
                    }
                }
                else if (request.CoverageTarget == "Category" && !string.IsNullOrWhiteSpace(request.CategoryId))
                {
                    var categoryExists = await _context.ItemCategories.AnyAsync(c => c.CategoryId == request.CategoryId);
                    if (!categoryExists)
                    {
                        return BadRequest(new { error = $"Category '{request.CategoryId}' not found" });
                    }
                }
                else
                {
                    return BadRequest(new { error = "Must specify either ItemId or CategoryId based on CoverageTarget" });
                }

                var newRule = new LoGCoverageRule
                {
                    LogId = log.LogId,
                    CoverageTarget = request.CoverageTarget,
                    ItemId = request.ItemId,
                    CategoryId = request.CategoryId,
                    CoverageType = request.CoverageType,
                    CoveragePercentage = request.CoveragePercentage,
                    CoverageFixedAmount = request.CoverageFixedAmount,
                    CapAmount = request.CapAmount,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _context.LoGCoverageRules.Add(newRule);
                await _context.SaveChangesAsync();

                // Reload log with all rules
                log = await _context.LogCoverages
                    .Include(l => l.CoverageRules)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LogId == logRecordId);

                var result = new LogDto
                {
                    LogId = log!.LogId,
                    SchoolYearId = log.SchoolYearId,
                    StudentId = log.StudentId,
                    SponsorId = log.SponsorId,
                    LogStatus = log.LogStatus,
                    IsActive = log.IsActive,
                    EffectiveFrom = log.EffectiveFrom,
                    EffectiveTo = log.EffectiveTo,
                    CreatedOn = log.CreatedOn,
                    ModifiedOn = log.ModifiedOn,
                    Items = log.CoverageRules.Select(r => new LogItemDto
                    {
                        RuleId = r.RuleId,
                        CoverageTarget = r.CoverageTarget,
                        ItemId = r.ItemId,
                        CategoryId = r.CategoryId,
                        CoverageType = r.CoverageType,
                        CoveragePercentage = r.CoveragePercentage,
                        CoverageFixedAmount = r.CoverageFixedAmount,
                        CapAmount = r.CapAmount
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to Letter of Guarantee {LogId}", logRecordId);
                return StatusCode(500, new
                {
                    error = "An error occurred adding the coverage item",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
