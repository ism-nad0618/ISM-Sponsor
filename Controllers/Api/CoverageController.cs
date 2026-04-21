using ISMSponsor.Constants;
using ISMSponsor.Data;
using ISMSponsor.Models.API;
using ISMSponsor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ISMSponsor.Controllers.Api
{
    [ApiController]
    [Route("api/v1/coverage")]
    [AllowAnonymous] // Demo: Allow Swagger testing without authentication
    // [Authorize(Roles = "admin,admissions,cashier")] // TODO: Re-enable for production
    [Produces("application/json")]
    public class CoverageController : ControllerBase
    {
        private readonly CoverageEvaluationService _evaluationService;
        private readonly AppDbContext _context;
        private readonly ILogger<CoverageController> _logger;

        public CoverageController(
            CoverageEvaluationService evaluationService,
            AppDbContext context,
            ILogger<CoverageController> logger)
        {
            _evaluationService = evaluationService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Evaluate and persist coverage decision to audit trail (commits decision)
        /// </summary>
        /// <param name="request">Coverage evaluation request with student, sponsor, and charge details</param>
        /// <returns>Coverage decision with allocation breakdown (persisted to audit)</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/coverage/evaluate
        ///     {
        ///       "studentId": "STU001",
        ///       "sponsorId": "ACME",
        ///       "schoolYearId": "2024-2025",
        ///       "itemId": "UNIFORMS",
        ///       "amount": 50000,
        ///       "chargeDate": "2024-01-15"
        ///     }
        ///
        /// Sample response:
        ///
        ///     {
        ///       "decisionId": "DEC-002",
        ///       "decision": "Covered",
        ///       "billTo": "Sponsor",
        ///       "sponsorAmount": 50000,
        ///       "parentAmount": 0,
        ///       "reasonCode": "FULL_COVERAGE",
        ///       "ruleVersion": "RV-1",
        ///       "auditRecordId": 1042,
        ///       "success": true
        ///     }
        ///
        /// Decision is persisted to audit trail and can be retrieved via GET /api/v1/audit/decisions/{decisionId}
        /// </remarks>
        [HttpPost("evaluate")]
        [ProducesResponseType(typeof(CoverageEvaluationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CoverageEvaluationResponse>> Evaluate([FromBody] CoverageEvaluationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var userDisplay = User.Identity?.Name ?? "Anonymous";
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "unknown";

            try
            {
                var response = await _evaluationService.EvaluateAsync(request, userId, userDisplay, userRole);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing coverage evaluation request");
                return StatusCode(500, new
                {
                    error = "An error occurred processing your request",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get all supported reason codes with descriptions
        /// </summary>
        /// <returns>List of reason codes with descriptions and categories</returns>
        [HttpGet("reasons")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(List<ReasonCodeInfo>), StatusCodes.Status200OK)]
        public ActionResult<List<ReasonCodeInfo>> GetReasonCodes()
        {
            var reasonCodes = CoverageReasonCodes.GetAllReasonCodes();
            return Ok(reasonCodes);
        }

        /// <summary>
        /// Preview coverage decision without persisting to audit trail (read-only evaluation)
        /// </summary>
        /// <param name="request">Coverage evaluation request with student, sponsor, and charge details</param>
        /// <returns>Coverage decision with allocation breakdown (not persisted)</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/coverage/preview
        ///     {
        ///       "studentId": "STU001",
        ///       "sponsorId": "ACME",
        ///       "schoolYearId": "2024-2025",
        ///       "itemId": "TUITION",
        ///       "amount": 100000,
        ///       "chargeDate": "2024-01-15"
        ///     }
        ///
        /// Sample response:
        ///
        ///     {
        ///       "decisionId": "DEC-001",
        ///       "decision": "Split",
        ///       "billTo": "Split",
        ///       "sponsorAmount": 75000,
        ///       "parentAmount": 25000,
        ///       "reasonCode": "CAP_PARTIAL",
        ///       "ruleVersion": "RV-1",
        ///       "success": true
        ///     }
        ///
        /// Use this endpoint for "what-if" analysis before committing a decision.
        /// </remarks>
        [HttpPost("preview")]
        [ProducesResponseType(typeof(CoverageEvaluationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CoverageEvaluationResponse>> Preview([FromBody] CoverageEvaluationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Enforce preview mode to prevent accidental audit logging
            request.IsPreview = true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var userDisplay = User.Identity?.Name ?? "Anonymous";
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "unknown";

            try
            {
                var response = await _evaluationService.EvaluateAsync(request, userId, userDisplay, userRole);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing coverage preview request");
                return StatusCode(500, new
                {
                    error = "An error occurred processing your preview request",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Retrieve a specific coverage decision by audit record ID
        /// </summary>
        /// <param name="auditId">Audit record ID returned from /evaluate or /commit (preview is not persisted)</param>
        /// <returns>Full coverage decision details including rule snapshot and percentages</returns>
        [HttpGet("decisions/{auditId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(CoverageDecisionDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CoverageDecisionDetailDto>> GetDecisionById(int auditId)
        {
            try
            {
                var audit = await _context.CoverageEvaluationAudits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AuditId == auditId);

                if (audit == null)
                {
                    return NotFound(new
                    {
                        error = $"Coverage decision with ID {auditId} not found",
                        auditId = auditId
                    });
                }

                var detail = MapAuditToDetail(audit);
                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coverage decision {AuditId}", auditId);
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving the decision",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Query coverage decisions by correlation ID for end-to-end traceability
        /// </summary>
        /// <param name="correlationId">Correlation ID propagated across systems</param>
        /// <param name="studentId">Optional: Filter by student ID</param>
        /// <param name="from">Optional: Start date for filtering decisions (UTC)</param>
        /// <param name="to">Optional: End date for filtering decisions (UTC)</param>
        /// <param name="limit">Maximum number of results (default 50, max 500)</param>
        /// <returns>List of coverage decisions matching the correlation ID</returns>
        [HttpGet("decisions")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(List<CoverageDecisionDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<CoverageDecisionDetailDto>>> GetDecisionsByCorrelationId(
            [FromQuery] string? correlationId,
            [FromQuery] string? studentId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
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

                var query = _context.CoverageEvaluationAudits.AsNoTracking();

                // Filter by correlation ID if provided
                if (!string.IsNullOrEmpty(correlationId))
                {
                    query = query.Where(a => a.CorrelationId == correlationId);
                }

                // Filter by student ID if provided
                if (!string.IsNullOrEmpty(studentId))
                {
                    query = query.Where(a => a.StudentId == studentId);
                }

                // Filter by date range if provided
                if (from.HasValue)
                {
                    query = query.Where(a => a.EvaluatedOn >= from.Value);
                }

                if (to.HasValue)
                {
                    query = query.Where(a => a.EvaluatedOn <= to.Value);
                }

                // Require at least one filter criteria
                if (string.IsNullOrEmpty(correlationId) && string.IsNullOrEmpty(studentId) && !from.HasValue && !to.HasValue)
                {
                    return BadRequest(new
                    {
                        error = "At least one filter criteria is required (correlationId, studentId, or dateRange)",
                        receivedParameters = new { correlationId, studentId, from, to }
                    });
                }

                var audits = await query
                    .OrderByDescending(a => a.EvaluatedOn)
                    .Take(limit)
                    .ToListAsync();

                var details = audits.Select(MapAuditToDetail).ToList();
                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying coverage decisions");
                return StatusCode(500, new
                {
                    error = "An error occurred querying coverage decisions",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Helper method to map CoverageEvaluationAudit to response DTO
        /// </summary>
        private static CoverageDecisionDetailDto MapAuditToDetail(Models.Domain.CoverageEvaluationAudit audit)
        {
            return new CoverageDecisionDetailDto
            {
                AuditId = audit.AuditId,
                CorrelationId = audit.CorrelationId,
                StudentId = audit.StudentId,
                SponsorId = audit.SponsorId,
                Amount = audit.RequestedAmount,
                ChargeDate = audit.ChargeDate,
                Decision = Enum.Parse<CoverageDecision>(audit.Decision),
                BillTo = Enum.Parse<BillTo>(audit.BillTo),
                SponsorAmount = audit.SponsorAmount,
                ParentAmount = audit.ParentAmount,
                SponsorPercent = audit.SponsorPercent ?? 0,
                ParentPercent = audit.ParentPercent ?? 0,
                ReasonCode = audit.ReasonCode,
                Explanation = audit.Explanation,
                RuleVersion = audit.RuleVersion,
                RuleSnapshot = audit.RuleSnapshot,
                EvaluatedOn = audit.EvaluatedOn,
                EvaluatedByUserId = audit.EvaluatedByUserId,
                EvaluatedByUserDisplay = audit.EvaluatedByUserDisplay,
                Success = audit.Success,
                ErrorMessage = audit.ErrorMessage
            };
        }
    }}