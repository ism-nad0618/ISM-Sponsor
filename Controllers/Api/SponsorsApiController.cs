using ISMSponsor.Data;
using ISMSponsor.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    /// <summary>
    /// REST API for sponsor data retrieval.
    /// Provides detailed sponsor information for external system consumption.
    /// </summary>
    [ApiController]
    [Route("api/sponsors")]
    [Route("api/v1/sponsors")]
    [Authorize(Roles = "admin,admissions,cashier,sponsor")]
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
        /// Get detailed sponsor information by ID.
        /// Returns full sponsor profile including contacts, addresses, and cross-system IDs.
        /// </summary>
        /// <param name="sponsorId">Sponsor ID</param>
        /// <returns>Detailed sponsor profile</returns>
        [HttpGet("{sponsorId}")]
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
