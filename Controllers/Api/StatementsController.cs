using ISMSponsor.Data;
using ISMSponsor.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    /// <summary>
    /// REST API for statement data retrieval.
    /// Provides coverage allocation data for statement presentation.
    /// </summary>
    [ApiController]
    [Route("api/statements")]
    [Route("api/v1/statements")]
    [Authorize(Roles = "admin,cashier,sponsor")]
    public class StatementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatementsController> _logger;

        public StatementsController(AppDbContext context, ILogger<StatementsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get statement data for a specific student.
        /// Returns all coverage decisions within the specified period.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="schoolYearId">Optional: Filter by school year</param>
        /// <param name="periodStart">Optional: Period start date (UTC)</param>
        /// <param name="periodEnd">Optional: Period end date (UTC)</param>
        /// <returns>Statement query result with line items</returns>
        [HttpGet("students/{studentId}")]
        [ProducesResponseType(typeof(StatementQueryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StatementQueryDto>> GetStudentStatement(
            string studentId,
            [FromQuery] string? schoolYearId,
            [FromQuery] DateTime? periodStart,
            [FromQuery] DateTime? periodEnd)
        {
            try
            {
                var query = _context.CoverageEvaluationAudits
                    .AsNoTracking()
                    .Where(a => a.StudentId == studentId && a.Success);

                if (!string.IsNullOrEmpty(schoolYearId))
                {
                    query = query.Where(a => a.SchoolYearId == schoolYearId);
                }

                if (periodStart.HasValue)
                {
                    query = query.Where(a => a.ChargeDate >= periodStart.Value);
                }

                if (periodEnd.HasValue)
                {
                    query = query.Where(a => a.ChargeDate <= periodEnd.Value);
                }

                var audits = await query
                    .OrderBy(a => a.ChargeDate)
                    .ToListAsync();

                var lineItems = audits.Select(a => new StatementLineItem
                {
                    AuditId = a.AuditId,
                    ChargeDate = a.ChargeDate,
                    ChargeDescription = a.ItemId ?? $"Charge #{a.AuditId}",
                    SponsorAmount = a.SponsorAmount,
                    ParentAmount = a.ParentAmount,
                    BillTo = Enum.Parse<BillTo>(a.BillTo),
                    ReasonCode = a.ReasonCode
                }).ToList();

                var result = new StatementQueryDto
                {
                    StudentId = studentId,
                    SchoolYearId = schoolYearId,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    LineItems = lineItems,
                    TotalSponsorAmount = lineItems.Sum(l => l.SponsorAmount),
                    TotalParentAmount = lineItems.Sum(l => l.ParentAmount),
                    GrandTotal = lineItems.Sum(l => l.SponsorAmount + l.ParentAmount)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student statement for {StudentId}", studentId);
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving the statement",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get statement data for a specific sponsor.
        /// Returns all coverage decisions where the sponsor is responsible.
        /// </summary>
        /// <param name="sponsorId">Sponsor ID</param>
        /// <param name="schoolYearId">Optional: Filter by school year</param>
        /// <param name="periodStart">Optional: Period start date (UTC)</param>
        /// <param name="periodEnd">Optional: Period end date (UTC)</param>
        /// <returns>Statement query result with line items</returns>
        [HttpGet("sponsors/{sponsorId}")]
        [ProducesResponseType(typeof(StatementQueryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StatementQueryDto>> GetSponsorStatement(
            string sponsorId,
            [FromQuery] string? schoolYearId,
            [FromQuery] DateTime? periodStart,
            [FromQuery] DateTime? periodEnd)
        {
            try
            {
                var query = _context.CoverageEvaluationAudits
                    .AsNoTracking()
                    .Where(a => a.SponsorId == sponsorId && a.Success && a.SponsorAmount > 0);

                if (!string.IsNullOrEmpty(schoolYearId))
                {
                    query = query.Where(a => a.SchoolYearId == schoolYearId);
                }

                if (periodStart.HasValue)
                {
                    query = query.Where(a => a.ChargeDate >= periodStart.Value);
                }

                if (periodEnd.HasValue)
                {
                    query = query.Where(a => a.ChargeDate <= periodEnd.Value);
                }

                var audits = await query
                    .OrderBy(a => a.ChargeDate)
                    .ToListAsync();

                var lineItems = audits.Select(a => new StatementLineItem
                {
                    AuditId = a.AuditId,
                    ChargeDate = a.ChargeDate,
                    ChargeDescription = a.ItemId ?? $"Charge #{a.AuditId}",
                    SponsorAmount = a.SponsorAmount,
                    ParentAmount = a.ParentAmount,
                    BillTo = Enum.Parse<BillTo>(a.BillTo),
                    ReasonCode = a.ReasonCode
                }).ToList();

                var result = new StatementQueryDto
                {
                    SponsorId = sponsorId,
                    SchoolYearId = schoolYearId,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    LineItems = lineItems,
                    TotalSponsorAmount = lineItems.Sum(l => l.SponsorAmount),
                    TotalParentAmount = lineItems.Sum(l => l.ParentAmount),
                    GrandTotal = lineItems.Sum(l => l.SponsorAmount + l.ParentAmount)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsor statement for {SponsorId}", sponsorId);
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving the statement",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
