using ISMSponsor.Data;
using ISMSponsor.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    /// <summary>
    /// REST API for sponsor reference data.
    /// Provides sponsor information for coverage evaluation and integration.
    /// </summary>
    [ApiController]
    [Route("api/v1/sponsors")]
    [AllowAnonymous] // Demo: Allow Swagger testing without authentication
    // [Authorize(Roles = "admin,admissions,cashier,sponsor")] // TODO: Re-enable for production
    [Produces("application/json")]
    public class SponsorsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SponsorsApiController> _logger;

        public SponsorsApiController(AppDbContext context, ILogger<SponsorsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get list of all active sponsors with basic information
        /// </summary>
        /// <param name="activeOnly">Filter to active sponsors only (default: true)</param>
        /// <returns>List of sponsors</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/sponsors?activeOnly=true
        ///
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(List<SponsorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<SponsorDto>>> GetSponsors([FromQuery] bool activeOnly = true)
        {
            try
            {
                var query = _context.Sponsors.AsNoTracking();

                if (activeOnly)
                {
                    query = query.Where(s => s.IsActive);
                }

                var sponsors = await query
                    .OrderBy(s => s.SponsorName)
                    .Select(s => new SponsorDto
                    {
                        SponsorId = s.SponsorId,
                        SponsorName = s.SponsorName,
                        LegalName = s.LegalName ?? string.Empty,
                        Tin = s.Tin ?? string.Empty,
                        IsActive = s.IsActive,
                        CreatedOn = s.CreatedOn,
                        ModifiedOn = s.ModifiedOn,
                        CrossSystemIds = new Dictionary<string, string>()
                    })
                    .ToListAsync();

                return Ok(sponsors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsors list");
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving sponsors",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Create a new sponsor
        /// </summary>
        /// <param name="request">Sponsor creation details</param>
        /// <returns>Created sponsor with confirmation</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/sponsors
        ///     {
        ///       "sponsorId": "NEWCORP",
        ///       "sponsorName": "New Corporation",
        ///       "legalName": "New Corporation Inc.",
        ///       "tin": "12-3456789",
        ///       "address": "123 Main St, City, State 12345"
        ///     }
        ///
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(SponsorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SponsorDto>> CreateSponsor([FromBody] CreateSponsorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if sponsor ID already exists
                var exists = await _context.Sponsors.AnyAsync(s => s.SponsorId == request.SponsorId);
                if (exists)
                {
                    return BadRequest(new { error = $"Sponsor with ID '{request.SponsorId}' already exists" });
                }

                var newSponsor = new Models.Domain.Sponsor
                {
                    SponsorId = request.SponsorId,
                    SponsorName = request.SponsorName,
                    LegalName = request.LegalName ?? request.SponsorName,
                    Tin = request.Tin ?? string.Empty,
                    Address = request.Address ?? string.Empty,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Sponsors.Add(newSponsor);
                await _context.SaveChangesAsync();

                var result = new SponsorDto
                {
                    SponsorId = newSponsor.SponsorId,
                    SponsorName = newSponsor.SponsorName,
                    LegalName = newSponsor.LegalName,
                    Tin = newSponsor.Tin,
                    IsActive = newSponsor.IsActive,
                    CreatedOn = newSponsor.CreatedOn,
                    ModifiedOn = newSponsor.ModifiedOn,
                    CrossSystemIds = new Dictionary<string, string>()
                };

                return CreatedAtAction(nameof(GetSponsorById), new { sponsorId = newSponsor.SponsorId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsor");
                return StatusCode(500, new
                {
                    error = "An error occurred creating the sponsor",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get detailed sponsor information by ID.
        /// Returns full sponsor profile including contacts, addresses, and cross-system IDs.
        /// </summary>
        /// <param name="sponsorId">Sponsor ID</param>
        /// <returns>Detailed sponsor profile</returns>
        [HttpGet("{sponsorId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(SponsorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SponsorDto>> GetSponsorById(string sponsorId)
        {
            try
            {
                var sponsor = await _context.Sponsors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

                if (sponsor == null)
                {
                    return NotFound(new
                    {
                        error = $"Sponsor with ID '{sponsorId}' not found",
                        sponsorId
                    });
                }

                var dto = new SponsorDto
                {
                    SponsorId = sponsor.SponsorId,
                    SponsorName = sponsor.SponsorName,
                    LegalName = sponsor.LegalName ?? string.Empty,
                    Tin = sponsor.Tin ?? string.Empty,
                    IsActive = sponsor.IsActive,
                    CreatedOn = sponsor.CreatedOn,
                    ModifiedOn = sponsor.ModifiedOn,
                    CrossSystemIds = new Dictionary<string, string>()
                };

                // Add cross-system IDs if they exist
                if (!string.IsNullOrEmpty(sponsor.PowerSchoolId))
                    dto.CrossSystemIds["PowerSchoolId"] = sponsor.PowerSchoolId;
                if (!string.IsNullOrEmpty(sponsor.NetSuiteId))
                    dto.CrossSystemIds["NetSuiteId"] = sponsor.NetSuiteId;
                if (!string.IsNullOrEmpty(sponsor.StudentChargingPortalId))
                    dto.CrossSystemIds["StudentChargingPortalId"] = sponsor.StudentChargingPortalId;
                if (!string.IsNullOrEmpty(sponsor.OnlineBillingSystemId))
                    dto.CrossSystemIds["OnlineBillingSystemId"] = sponsor.OnlineBillingSystemId;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsor {SponsorId}", sponsorId);
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving the sponsor",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
