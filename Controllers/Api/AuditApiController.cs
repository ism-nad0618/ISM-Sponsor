using ISMSponsor.Data;
using ISMSponsor.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    /// <summary>
    /// REST API for audit trail and decision traceability.
    /// Provides endpoints for querying coverage decisions and integration sync history.
    /// </summary>
    [ApiController]
    [Route("api/audit")]
    [Route("api/v1/audit")]
    [Authorize(Roles = "admin,cashier")]
    public class AuditApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuditApiController> _logger;

        public AuditApiController(AppDbContext context, ILogger<AuditApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get detailed coverage decision by decision ID (AuditId).
        /// Alias for /api/coverage/decisions/{decisionId} to provide audit-centric routing.
        /// </summary>
        /// <param name="decisionId">Coverage decision audit ID</param>
        /// <returns>Detailed coverage decision with audit trail</returns>
        [HttpGet("decisions/{decisionId}")]
        [ProducesResponseType(typeof(CoverageDecisionDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CoverageDecisionDetailDto>> GetDecisionById(int decisionId)
        {
            try
            {
                var audit = await _context.CoverageEvaluationAudits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AuditId == decisionId);

                if (audit == null)
                {
                    return NotFound(new
                    {
                        error = $"Coverage decision with ID {decisionId} not found",
                        decisionId
                    });
                }

                var detail = MapAuditToDetail(audit);
                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coverage decision {DecisionId}", decisionId);
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving the decision",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get integration sync history by correlation ID.
        /// Returns all downstream system sync attempts for a given correlation ID.
        /// </summary>
        /// <param name="correlationId">Correlation ID for end-to-end traceability</param>
        /// <returns>Sync status across all downstream systems</returns>
        [HttpGet("integrations/{correlationId}")]
        [ProducesResponseType(typeof(SyncStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<SyncStatusDto>> GetIntegrationSyncStatus(string correlationId)
        {
            try
            {
                var logs = await _context.SyncLogs
                    .AsNoTracking()
                    .Where(s => s.CorrelationId == correlationId)
                    .OrderByDescending(s => s.AttemptedAt)
                    .ToListAsync();

                if (!logs.Any())
                {
                    return NotFound(new { error = $"No sync records found for correlationId {correlationId}" });
                }

                var systems = new[]
                {
                    Integration.Orchestration.IntegrationTargets.PowerSchool,
                    Integration.Orchestration.IntegrationTargets.StudentChargingPortal,
                    Integration.Orchestration.IntegrationTargets.NetSuite,
                    Integration.Orchestration.IntegrationTargets.OnlineBillingSystem
                };

                var targetSystems = systems.Select(system =>
                {
                    var latest = logs.FirstOrDefault(l => l.TargetSystem == system);
                    return new TargetSystemStatusDto
                    {
                        System = system,
                        Status = latest?.Status ?? "Pending",
                        LastAttemptedAt = latest?.AttemptedAt,
                        LastSucceededAt = latest?.LastSucceededAt,
                        RetryCount = latest?.RetryCount ?? 0,
                        ErrorMessage = latest?.ErrorMessage,
                        ExternalReferenceId = latest?.ExternalReferenceId
                    };
                }).ToList();

                var overallStatus = ComputeOverallStatus(targetSystems);
                var lastUpdated = logs.Max(l => l.AttemptedAt);

                return Ok(new SyncStatusDto
                {
                    CorrelationId = correlationId,
                    TargetSystems = targetSystems,
                    OverallStatus = overallStatus,
                    LastUpdatedAt = lastUpdated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving integration sync status for {CorrelationId}", correlationId);
                return StatusCode(500, new
                {
                    error = "An error occurred retrieving sync status",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

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

        private static string ComputeOverallStatus(List<TargetSystemStatusDto> statuses)
        {
            if (statuses.All(s => s.Status == "Pending")) return "Pending";
            if (statuses.Any(s => s.Status == "InProgress")) return "InProgress";

            var hasFailed = statuses.Any(s => s.Status == "Failed");
            var hasSucceeded = statuses.Any(s => s.Status == "Succeeded" || s.Status == "Skipped");

            if (hasFailed && hasSucceeded) return "PartialSuccess";
            if (hasFailed) return "Failed";
            if (statuses.All(s => s.Status == "Succeeded" || s.Status == "Skipped")) return "Succeeded";

            return "InProgress";
        }
    }
}
