using ISMSponsor.Data;
using ISMSponsor.Integration.Adapters;
using ISMSponsor.Integration.Orchestration;
using ISMSponsor.Models.API;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers.Api
{
    [ApiController]
    [Route("api")]
    [Route("api/v1/integrations")]
    [Authorize(Roles = "admin,cashier")]
    public class IntegrationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INetSuiteAdapter _netSuiteAdapter;
        private readonly IOnlineBillingSystemAdapter _obsAdapter;
        private readonly IPowerSchoolAdapter _powerSchoolAdapter;
        private readonly IIntegrationSyncService _integrationSyncService;
        private readonly ILogger<IntegrationController> _logger;

        public IntegrationController(
            AppDbContext context,
            INetSuiteAdapter netSuiteAdapter,
            IOnlineBillingSystemAdapter obsAdapter,
            IPowerSchoolAdapter powerSchoolAdapter,
            IIntegrationSyncService integrationSyncService,
            ILogger<IntegrationController> logger)
        {
            _context = context;
            _netSuiteAdapter = netSuiteAdapter;
            _obsAdapter = obsAdapter;
            _powerSchoolAdapter = powerSchoolAdapter;
            _integrationSyncService = integrationSyncService;
            _logger = logger;
        }

        /// <summary>
        /// Sync student-sponsor links from PowerSchool to ISM Sponsor system.
        /// Creates or updates Letter of Guarantee assignments based on PowerSchool data.
        /// </summary>
        /// <param name="request">Student-sponsor sync request from PowerSchool</param>
        /// <returns>Sync result with detailed mapping outcomes</returns>
        [HttpPost("powerschool/student-sponsor-sync")]
        [ProducesResponseType(typeof(StudentSponsorSyncResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StudentSponsorSyncResponse>> SyncStudentSponsorsFromPowerSchool(
            [FromBody] StudentSponsorSyncRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();
            var results = new List<MappingSyncResult>();

            try
            {
                foreach (var mapping in request.Mappings)
                {
                    try
                    {
                        // Verify sponsor exists
                        var sponsor = await _context.Sponsors
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.SponsorId == mapping.SponsorId && s.IsActive);

                        if (sponsor == null)
                        {
                            results.Add(new MappingSyncResult
                            {
                                StudentId = mapping.StudentId,
                                SponsorId = mapping.SponsorId,
                                Success = false,
                                Status = "Failed",
                                ErrorMessage = $"Sponsor '{mapping.SponsorId}' not found or inactive"
                            });
                            continue;
                        }

                        // Verify or create student record
                        var student = await _context.Students
                            .FirstOrDefaultAsync(s => s.StudentId == mapping.StudentId);

                        if (student == null)
                        {
                            // Student doesn't exist - skip this mapping as students should be synced from PowerSchool first
                            results.Add(new MappingSyncResult
                            {
                                StudentId = mapping.StudentId,
                                SponsorId = mapping.SponsorId,
                                Success = false,
                                Status = "Failed",
                                ErrorMessage = $"Student '{mapping.StudentId}' not found in system"
                            });
                            continue;
                        }

                        // Check if LoG already exists for this student-sponsor-schoolyear combination
                        var existingLog = await _context.LogCoverages
                            .FirstOrDefaultAsync(l =>
                                l.StudentId == mapping.StudentId &&
                                l.SponsorId == mapping.SponsorId &&
                                l.SchoolYearId == request.SchoolYearId &&
                                l.IsActive);

                        if (existingLog != null)
                        {
                            // Update existing LoG if dates changed
                            existingLog.EffectiveFrom = mapping.EffectiveFrom;
                            existingLog.EffectiveTo = mapping.EffectiveTo;
                            existingLog.ModifiedOn = DateTime.UtcNow;
                            await _context.SaveChangesAsync();

                            results.Add(new MappingSyncResult
                            {
                                StudentId = mapping.StudentId,
                                SponsorId = mapping.SponsorId,
                                Success = true,
                                Status = "Updated",
                                Message = "Existing LoG updated",
                                LogId = existingLog.LogId
                            });
                        }
                        else
                        {
                            // Create new LoG
                            var newLog = new LogCoverage
                            {
                                StudentId = mapping.StudentId,
                                SponsorId = mapping.SponsorId,
                                SchoolYearId = request.SchoolYearId,
                                EffectiveFrom = mapping.EffectiveFrom,
                                EffectiveTo = mapping.EffectiveTo,
                                IsActive = true,
                                LogStatus = "Approved",
                                CreatedOn = DateTime.UtcNow
                            };
                            _context.LogCoverages.Add(newLog);
                            await _context.SaveChangesAsync();

                            results.Add(new MappingSyncResult
                            {
                                StudentId = mapping.StudentId,
                                SponsorId = mapping.SponsorId,
                                Success = true,
                                Status = "Created",
                                Message = "New LoG created",
                                LogId = newLog.LogId
                            });
                        }

                        // Sync to PowerSchool adapter
                        await _powerSchoolAdapter.SyncStudentTagsAsync(mapping.StudentId, mapping.SponsorId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing mapping for Student={StudentId}, Sponsor={SponsorId}",
                            mapping.StudentId, mapping.SponsorId);
                        results.Add(new MappingSyncResult
                        {
                            StudentId = mapping.StudentId,
                            SponsorId = mapping.SponsorId,
                            Success = false,
                            Status = "Failed",
                            ErrorMessage = ex.Message
                        });
                    }
                }

                var response = new StudentSponsorSyncResponse
                {
                    CorrelationId = correlationId,
                    Success = results.All(r => r.Success),
                    TotalMappings = request.Mappings.Count,
                    SuccessfulMappings = results.Count(r => r.Success),
                    FailedMappings = results.Count(r => !r.Success),
                    Results = results,
                    Message = $"Processed {request.Mappings.Count} mappings: {results.Count(r => r.Success)} succeeded, {results.Count(r => !r.Success)} failed"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PowerSchool student-sponsor sync");
                return StatusCode(500, new
                {
                    error = "An error occurred processing the sync request",
                    requestId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("netsuite/post-decision")]
        [HttpPost("netsuite/allocation-post")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostDecisionToNetSuite([FromBody] CoverageDecisionSyncRequestDto request)
        {
            var audit = await _context.CoverageEvaluationAudits
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuditId == request.AuditId);

            if (audit == null)
            {
                return NotFound(new { error = $"Coverage decision {request.AuditId} not found" });
            }

            if (string.IsNullOrWhiteSpace(audit.SponsorId))
            {
                return BadRequest(new { error = "Coverage decision has no SponsorId and cannot be posted to NetSuite" });
            }

            var correlationId = audit.CorrelationId ?? Guid.NewGuid().ToString();
            var eventType = "CoverageDecisionPost";
            var result = await _netSuiteAdapter.UpdateBillingAllocationAsync(audit.SponsorId, audit.SponsorAmount);

            await LogSyncAsync(
                audit,
                IntegrationTargets.NetSuite,
                eventType,
                correlationId,
                result,
                new { audit.AuditId, audit.SponsorId, audit.SponsorAmount, audit.ParentAmount, audit.BillTo });

            return Ok(new
            {
                auditId = audit.AuditId,
                correlationId,
                targetSystem = IntegrationTargets.NetSuite,
                result.Success,
                result.Status,
                result.Message,
                result.ExternalReferenceId,
                result.ErrorMessage
            });
        }

        [HttpPost("obs/post-decision")]
        [HttpPost("obs/statement-update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostDecisionToObs([FromBody] CoverageDecisionSyncRequestDto request)
        {
            var audit = await _context.CoverageEvaluationAudits
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuditId == request.AuditId);

            if (audit == null)
            {
                return NotFound(new { error = $"Coverage decision {request.AuditId} not found" });
            }

            if (string.IsNullOrWhiteSpace(audit.SponsorId))
            {
                return BadRequest(new { error = "Coverage decision has no SponsorId and cannot be posted to OBS" });
            }

            var correlationId = audit.CorrelationId ?? Guid.NewGuid().ToString();
            var eventType = "CoverageDecisionPost";

            var result = audit.SponsorAmount > 0
                ? await _obsAdapter.SyncCoveredStudentsAsync(audit.SponsorId, new List<string> { audit.StudentId })
                : new Integration.Contracts.SyncResult
                {
                    Success = true,
                    Status = "Skipped",
                    Message = "Sponsor amount is zero; no covered student sync required",
                    ProcessedAt = DateTime.UtcNow
                };

            await LogSyncAsync(
                audit,
                IntegrationTargets.OnlineBillingSystem,
                eventType,
                correlationId,
                result,
                new { audit.AuditId, audit.SponsorId, audit.StudentId, audit.SponsorAmount, audit.ParentAmount, audit.BillTo });

            return Ok(new
            {
                auditId = audit.AuditId,
                correlationId,
                targetSystem = IntegrationTargets.OnlineBillingSystem,
                result.Success,
                result.Status,
                result.Message,
                result.ExternalReferenceId,
                result.ErrorMessage
            });
        }

        [HttpGet("netsuite/posting-status/{correlationId}")]
        [HttpGet("sync-status/{correlationId}")]
        [ProducesResponseType(typeof(SyncStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SyncStatusDto>> GetSyncStatus(string correlationId)
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
                IntegrationTargets.PowerSchool,
                IntegrationTargets.StudentChargingPortal,
                IntegrationTargets.NetSuite,
                IntegrationTargets.OnlineBillingSystem
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

        private async Task LogSyncAsync(
            CoverageEvaluationAudit audit,
            string targetSystem,
            string eventType,
            string correlationId,
            Integration.Contracts.SyncResult result,
            object requestPayload)
        {
            var log = new SyncLog
            {
                EntityType = "CoverageDecision",
                EntityId = audit.AuditId.ToString(),
                TargetSystem = targetSystem,
                EventType = eventType,
                Status = string.IsNullOrWhiteSpace(result.Status) ? (result.Success ? "Succeeded" : "Failed") : result.Status,
                AttemptedAt = DateTime.UtcNow,
                LastSucceededAt = result.Success ? DateTime.UtcNow : null,
                RetryCount = 0,
                ErrorMessage = result.ErrorMessage,
                CorrelationId = correlationId,
                ExternalReferenceId = result.ExternalReferenceId,
                RequestPayload = System.Text.Json.JsonSerializer.Serialize(requestPayload),
                ResponsePayload = result.Message,
                CreatedOn = DateTime.UtcNow
            };

            _context.SyncLogs.Add(log);
            await _context.SaveChangesAsync();
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
