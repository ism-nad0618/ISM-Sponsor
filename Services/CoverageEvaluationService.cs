using ISMSponsor.Constants;
using ISMSponsor.Data;
using ISMSponsor.Models.API;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ISMSponsor.Services
{
    public class CoverageEvaluationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CoverageEvaluationService> _logger;

        public CoverageEvaluationService(AppDbContext context, ILogger<CoverageEvaluationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CoverageEvaluationResponse> EvaluateAsync(
            CoverageEvaluationRequest request,
            string userId,
            string userDisplay,
            string userRole)
        {
            var response = new CoverageEvaluationResponse { Success = true };
            CoverageEvaluationAudit audit = new();

            try
            {
                // Step 1: Validate request
                var validationError = ValidateRequest(request);
                if (validationError != null)
                {
                    response.Success = false;
                    response.ErrorMessage = validationError.Value.Message;
                    response.ReasonCode = validationError.Value.Code;
                    response.Explanation = validationError.Value.Message;
                    response.Decision = CoverageDecision.NotCovered;
                    response.BillTo = Models.API.BillTo.Parent;
                    response.ParentAmount = request.Amount;

                    PopulateEvaluationMetadata(request, response);
                    audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                    audit.CorrelationId = response.CorrelationId;
                    audit.SponsorPercent = response.SponsorPercent;
                    audit.ParentPercent = response.ParentPercent;
                    if (!request.IsPreview)
                    {
                        _context.CoverageEvaluationAudits.Add(audit);
                        await _context.SaveChangesAsync();
                        response.AuditRecordId = audit.AuditId;
                    }
                    else
                    {
                        response.AuditRecordId = 0;
                    }
                    return response;
                }

                // Step 2: Find active LoG for student
                var log = await FindActiveLogAsync(request);
                if (log == null)
                {
                    // Try to get student info even though there's no LoG
                    var student = await _context.Students
                        .Where(s => s.StudentId == request.StudentId && s.SchoolYearId == request.SchoolYearId)
                        .FirstOrDefaultAsync();
                    
                    response.Decision = CoverageDecision.NotCovered;
                    response.BillTo = Models.API.BillTo.Parent;
                    response.ParentAmount = request.Amount;
                    response.TotalAmount = request.Amount;
                    response.StudentId = request.StudentId;
                    response.StudentName = student != null ? $"{student.FirstName} {student.LastName}" : null;
                    response.ReasonCode = CoverageReasonCodes.NO_ACTIVE_LOG;
                    response.Explanation = $"No active Letter of Guarantee found for student {request.StudentId} in school year {request.SchoolYearId}";

                    PopulateEvaluationMetadata(request, response);
                    
                    // Create single parent allocation
                    response.Allocations = new List<BillingAllocationDto>
                    {
                        new BillingAllocationDto
                        {
                            PartyType = "Parent",
                            PartyId = null,
                            PartyName = null,
                            Amount = Math.Round(response.ParentAmount, 2),
                            Currency = request.Currency ?? "US Dollar",
                            BillTo = "Parent",
                            ChargeCode = request.ItemId ?? request.CategoryId ?? string.Empty,
                            ChargeDescription = request.ChargeDescription
                        }
                    };
                    
                    audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                    audit.CorrelationId = response.CorrelationId;
                    audit.SponsorPercent = response.SponsorPercent;
                    audit.ParentPercent = response.ParentPercent;
                    if (!request.IsPreview)
                    {
                        _context.CoverageEvaluationAudits.Add(audit);
                        await _context.SaveChangesAsync();
                        response.AuditRecordId = audit.AuditId;
                    }
                    else
                    {
                        response.AuditRecordId = 0;
                    }
                    return response;
                }

                // Step 3: Verify LoG is active
                if (!log.IsActive)
                {
                    response.Decision = CoverageDecision.NotCovered;
                    response.BillTo = Models.API.BillTo.Parent;
                    response.ParentAmount = request.Amount;
                    response.ReasonCode = CoverageReasonCodes.LOG_INACTIVE;
                    response.Explanation = $"Letter of Guarantee (ID: {log.LogId}) is inactive";

                    PopulateEvaluationMetadata(request, response);
                    audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                    audit.LogId = log.LogId;
                    audit.CorrelationId = response.CorrelationId;
                    audit.SponsorPercent = response.SponsorPercent;
                    audit.ParentPercent = response.ParentPercent;
                    _context.CoverageEvaluationAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    response.AuditRecordId = audit.AuditId;
                    return response;
                }

                // Step 4: Validate sponsor/log consistency if both provided
                if (!string.IsNullOrEmpty(request.SponsorId) && request.SponsorId != log.SponsorId)
                {
                    response.Success = false;
                    response.ErrorMessage = $"Provided SponsorId '{request.SponsorId}' does not match LoG sponsor '{log.SponsorId}'";
                    response.ReasonCode = CoverageReasonCodes.SPONSOR_LOG_MISMATCH;
                    response.Explanation = response.ErrorMessage;
                    response.Decision = CoverageDecision.NotCovered;
                    response.BillTo = Models.API.BillTo.Parent;
                    response.ParentAmount = request.Amount;

                    PopulateEvaluationMetadata(request, response);
                    audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                    audit.LogId = log.LogId;
                    audit.CorrelationId = response.CorrelationId;
                    audit.SponsorPercent = response.SponsorPercent;
                    audit.ParentPercent = response.ParentPercent;
                    _context.CoverageEvaluationAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    response.AuditRecordId = audit.AuditId;
                    return response;
                }

                // Step 5: Find matching rule (item-level first, then category-level)
                var matchedRule = await FindMatchingRuleAsync(log, request);
                if (matchedRule == null)
                {
                    response.Decision = CoverageDecision.NotCovered;
                    response.BillTo = Models.API.BillTo.Parent;
                    response.ParentAmount = request.Amount;
                    response.ReasonCode = CoverageReasonCodes.NO_MATCHING_RULE;
                    response.Explanation = $"No coverage rule found for the specified item or category";

                    PopulateEvaluationMetadata(request, response);
                    audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                    audit.LogId = log.LogId;
                    audit.CorrelationId = response.CorrelationId;
                    audit.SponsorPercent = response.SponsorPercent;
                    audit.ParentPercent = response.ParentPercent;
                    _context.CoverageEvaluationAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    response.AuditRecordId = audit.AuditId;
                    return response;
                }

                // Step 6: Verify rule is active and within effective dates
                var ruleValidation = ValidateRule(matchedRule, request.ChargeDate);
                if (!ruleValidation.IsValid)
                {
                    response.Decision = CoverageDecision.NotCovered;
                    response.BillTo = Models.API.BillTo.Parent;
                    response.ParentAmount = request.Amount;
                    response.ReasonCode = ruleValidation.ReasonCode;
                    response.Explanation = ruleValidation.Explanation;

                    PopulateEvaluationMetadata(request, response);
                    audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                    audit.LogId = log.LogId;
                    audit.MatchedRuleId = matchedRule.RuleId;
                    audit.CorrelationId = response.CorrelationId;
                    audit.SponsorPercent = response.SponsorPercent;
                    audit.ParentPercent = response.ParentPercent;
                    _context.CoverageEvaluationAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    response.AuditRecordId = audit.AuditId;
                    return response;
                }

                // Step 7: Calculate coverage based on rule type
                var allocation = CalculateCoverage(matchedRule, request.Amount);
                response.Decision = allocation.Decision;
                response.BillTo = allocation.BillTo;
                response.SponsorAmount = allocation.SponsorAmount;
                response.ParentAmount = allocation.ParentAmount;
                response.ReasonCode = allocation.ReasonCode;
                response.Explanation = allocation.Explanation;
                response.MatchedRuleId = matchedRule.RuleId;
                response.RuleVersion = GenerateRuleVersion(log, matchedRule);

                // Step 8: Populate new response fields for audit trail and traceability
                response.EvaluatedAt = DateTime.UtcNow;
                response.CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString();
                response.DecisionId = Guid.NewGuid().ToString();
                
                // Calculate coverage percentages
                if (request.Amount > 0)
                {
                    response.SponsorPercent = Math.Round((response.SponsorAmount / request.Amount) * 100, 2);
                    response.ParentPercent = Math.Round((response.ParentAmount / request.Amount) * 100, 2);
                }
                
                // Populate sponsor information and total amount
                response.StudentId = log.StudentId;
                response.StudentName = log.Student != null ? $"{log.Student.FirstName} {log.Student.LastName}" : null;
                response.SponsorId = log.SponsorId;
                response.SponsorName = log.Sponsor?.SponsorName;
                response.TotalAmount = request.Amount;

                // Serialize the matched rule as snapshot for audit compliance and rule replay
                response.RuleSnapshot = SerializeRuleSnapshot(log, matchedRule, request, response);

                // Create billing allocations based on decision type
                if (response.Decision == CoverageDecision.Covered && response.BillTo == Models.API.BillTo.Sponsor)
                {
                    // Fully covered by sponsor: single sponsor allocation
                    response.Allocations = new List<BillingAllocationDto>
                    {
                        new BillingAllocationDto
                        {
                            PartyType = "Sponsor",
                            PartyId = log.SponsorId,
                            PartyName = log.Sponsor?.SponsorName,
                            Amount = Math.Round(response.SponsorAmount, 2),
                            Currency = request.Currency ?? "US Dollar",
                            BillTo = "Sponsor",
                            ChargeCode = request.ItemId ?? request.CategoryId ?? string.Empty,
                            ChargeDescription = request.ChargeDescription
                        }
                    };
                }
                else if (response.Decision == CoverageDecision.Split || response.BillTo == Models.API.BillTo.SponsorAndParent)
                {
                    // Split billing: two allocations (sponsor and parent)
                    response.Allocations = new List<BillingAllocationDto>
                    {
                        new BillingAllocationDto
                        {
                            PartyType = "Sponsor",
                            PartyId = log.SponsorId,
                            PartyName = log.Sponsor?.SponsorName,
                            Amount = Math.Round(response.SponsorAmount, 2),
                            Currency = request.Currency ?? "US Dollar",
                            BillTo = "Sponsor",
                            ChargeCode = request.ItemId ?? request.CategoryId ?? string.Empty,
                            ChargeDescription = request.ChargeDescription
                        },
                        new BillingAllocationDto
                        {
                            PartyType = "Parent",
                            PartyId = null,
                            PartyName = null,
                            Amount = Math.Round(response.ParentAmount, 2),
                            Currency = request.Currency ?? "US Dollar",
                            BillTo = "Parent",
                            ChargeCode = request.ItemId ?? request.CategoryId ?? string.Empty,
                            ChargeDescription = request.ChargeDescription
                        }
                    };
                }
                else if (response.Decision == CoverageDecision.NotCovered && response.BillTo == Models.API.BillTo.Parent)
                {
                    // Not covered: single parent allocation
                    response.Allocations = new List<BillingAllocationDto>
                    {
                        new BillingAllocationDto
                        {
                            PartyType = "Parent",
                            PartyId = null,
                            PartyName = null,
                            Amount = Math.Round(response.ParentAmount, 2),
                            Currency = request.Currency ?? "US Dollar",
                            BillTo = "Parent",
                            ChargeCode = request.ItemId ?? request.CategoryId ?? string.Empty,
                            ChargeDescription = request.ChargeDescription
                        }
                    };
                }

                // Step 9: Create audit record (skip for preview requests)
                audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                audit.LogId = log.LogId;
                audit.MatchedRuleId = matchedRule.RuleId;
                audit.RuleVersion = response.RuleVersion;
                audit.CorrelationId = response.CorrelationId;
                audit.SponsorPercent = response.SponsorPercent;
                audit.ParentPercent = response.ParentPercent;
                audit.RuleSnapshot = response.RuleSnapshot;
                
                // Only persist audit record if this is not a preview request
                if (!request.IsPreview)
                {
                    _context.CoverageEvaluationAudits.Add(audit);
                    await _context.SaveChangesAsync();
                    response.AuditRecordId = audit.AuditId;
                }
                else
                {
                    // For preview, still generate an ID for response but don't persist
                    response.AuditRecordId = 0; // 0 indicates this is a preview, not persisted
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating coverage for student {StudentId}", request.StudentId);
                response.Success = false;
                response.ErrorMessage = "An error occurred during coverage evaluation";
                response.Decision = CoverageDecision.NotCovered;
                response.BillTo = Models.API.BillTo.Parent;
                response.ParentAmount = request.Amount;
                response.ReasonCode = "SYSTEM_ERROR";
                response.Explanation = "System error during evaluation";
                
                // Populate audit fields for error scenario
                response.EvaluatedAt = DateTime.UtcNow;
                response.CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString();
                response.DecisionId = Guid.NewGuid().ToString();
                
                // Calculate coverage percentages (should be 0/100 for error case)
                if (request.Amount > 0)
                {
                    response.ParentPercent = 100;
                    response.SponsorPercent = 0;
                }

                audit = CreateAuditRecord(request, response, userId, userDisplay, userRole);
                audit.ErrorMessage = ex.Message;
                audit.CorrelationId = response.CorrelationId;
                audit.SponsorPercent = response.SponsorPercent;
                audit.ParentPercent = response.ParentPercent;
                _context.CoverageEvaluationAudits.Add(audit);
                await _context.SaveChangesAsync();
                response.AuditRecordId = audit.AuditId;

                return response;
            }
        }

        private (string Code, string Message)? ValidateRequest(CoverageEvaluationRequest request)
        {
            if (string.IsNullOrEmpty(request.StudentId))
                return (CoverageReasonCodes.INVALID_STUDENT, "Student ID is required");

            if (string.IsNullOrEmpty(request.SchoolYearId))
                return (CoverageReasonCodes.INVALID_STUDENT, "School year is required");

            if (request.Amount <= 0)
                return (CoverageReasonCodes.INVALID_AMOUNT, "Amount must be greater than zero");

            if (string.IsNullOrEmpty(request.ItemId) && string.IsNullOrEmpty(request.CategoryId))
                return (CoverageReasonCodes.MISSING_ITEM_OR_CATEGORY, "Either ItemId or CategoryId must be provided");

            return null;
        }

        private async Task<LogCoverage?> FindActiveLogAsync(CoverageEvaluationRequest request)
        {
            var query = _context.LogCoverages
                .Include(l => l.Sponsor)
                .Include(l => l.Student)
                .Include(l => l.CoverageRules!)
                .ThenInclude(r => r.Item)
                .Include(l => l.CoverageRules!)
                .ThenInclude(r => r.Category)
                .Where(l => l.SchoolYearId == request.SchoolYearId && l.StudentId == request.StudentId);

            if (request.LogId.HasValue)
            {
                query = query.Where(l => l.LogId == request.LogId.Value);
            }
            else if (!string.IsNullOrEmpty(request.SponsorId))
            {
                query = query.Where(l => l.SponsorId == request.SponsorId);
            }

            // Prefer active LoGs, then sort by most recent
            var logs = await query
                .OrderByDescending(l => l.IsActive)
                .ThenByDescending(l => l.LogId)
                .ToListAsync();

            return logs.FirstOrDefault(l => l.IsActive);
        }

        private async Task<LoGCoverageRule?> FindMatchingRuleAsync(LogCoverage log, CoverageEvaluationRequest request)
        {
            if (log.CoverageRules == null || !log.CoverageRules.Any())
                return null;

            var activeRules = log.CoverageRules.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToList();

            // Fast preview path: Check common item rules first for quick resolution
            if (!string.IsNullOrEmpty(request.ItemId))
            {
                var itemRule = activeRules.FirstOrDefault(r =>
                    r.CoverageTarget == "Item" &&
                    r.ItemId == request.ItemId);

                if (itemRule != null)
                    return itemRule;
            }

            // Then check category rules
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                var categoryRule = activeRules.FirstOrDefault(r =>
                    r.CoverageTarget == "Category" &&
                    r.CategoryId == request.CategoryId);

                if (categoryRule != null)
                    return categoryRule;
            }

            // If ItemId was provided but no direct rule, check if that item belongs to a covered category
            if (!string.IsNullOrEmpty(request.ItemId))
            {
                var item = await _context.Items
                    .Where(i => i.ItemId == request.ItemId)
                    .Select(i => new { i.CategoryId })
                    .FirstOrDefaultAsync();

                if (item?.CategoryId != null)
                {
                    var categoryRule = activeRules.FirstOrDefault(r =>
                        r.CoverageTarget == "Category" &&
                        r.CategoryId == item.CategoryId);

                    if (categoryRule != null)
                        return categoryRule;
                }
            }

            return null;
        }

        private (bool IsValid, string ReasonCode, string Explanation) ValidateRule(LoGCoverageRule rule, DateTime chargeDate)
        {
            if (!rule.IsActive)
                return (false, CoverageReasonCodes.RULE_INACTIVE, "The matching rule is inactive");

            if (rule.EffectiveFrom.HasValue && chargeDate < rule.EffectiveFrom.Value)
                return (false, CoverageReasonCodes.RULE_NOT_YET_EFFECTIVE,
                    $"The matching rule is not effective until {rule.EffectiveFrom.Value:yyyy-MM-dd}");

            if (rule.EffectiveTo.HasValue && chargeDate > rule.EffectiveTo.Value)
                return (false, CoverageReasonCodes.RULE_EXPIRED,
                    $"The matching rule expired on {rule.EffectiveTo.Value:yyyy-MM-dd}");

            return (true, string.Empty, string.Empty);
        }

        private (CoverageDecision Decision, Models.API.BillTo BillTo, decimal SponsorAmount, decimal ParentAmount, string ReasonCode, string Explanation) 
            CalculateCoverage(LoGCoverageRule rule, decimal requestedAmount)
        {
            var isItemRule = rule.CoverageTarget == "Item";
            var targetDescription = isItemRule
                ? $"item '{rule.Item?.ItemName ?? rule.ItemId}'"
                : $"category '{rule.Category?.CategoryName ?? rule.CategoryId}'";

            switch (rule.CoverageType)
            {
                case "Full":
                    return (
                        CoverageDecision.Covered,
                        Models.API.BillTo.Sponsor,
                        requestedAmount,
                        0,
                        CoverageReasonCodes.COVERED,
                        "The charge is fully covered by the sponsor."
                    );

                case "Percentage":
                    var percentage = rule.CoveragePercentage ?? 0;
                    var sponsorAmount = Math.Round(requestedAmount * percentage / 100, 2);
                    var parentAmount = requestedAmount - sponsorAmount;

                    if (percentage >= 100)
                    {
                        return (
                            CoverageDecision.Covered,
                            Models.API.BillTo.Sponsor,
                            requestedAmount,
                            0,
                            CoverageReasonCodes.COVERED,
                            "The charge is fully covered by the sponsor."
                        );
                    }
                    else
                    {
                        return (
                            CoverageDecision.Split,
                            Models.API.BillTo.SponsorAndParent,
                            sponsorAmount,
                            parentAmount,
                            CoverageReasonCodes.PERCENTAGE_SPLIT,
                            $"Partial coverage ({percentage}%) applied for {targetDescription}: Sponsor pays ₱{sponsorAmount:N2}, Parent pays ₱{parentAmount:N2}"
                        );
                    }

                case "FixedAmount":
                    var fixedAmount = rule.CoverageFixedAmount ?? 0;
                    if (fixedAmount >= requestedAmount)
                    {
                        return (
                            CoverageDecision.Covered,
                            Models.API.BillTo.Sponsor,
                            requestedAmount,
                            0,
                            CoverageReasonCodes.COVERED,
                            "The charge is fully covered by the sponsor."
                        );
                    }
                    else
                    {
                        return (
                            CoverageDecision.Split,
                            Models.API.BillTo.SponsorAndParent,
                            fixedAmount,
                            requestedAmount - fixedAmount,
                            CoverageReasonCodes.FIXED_SPLIT,
                            $"Fixed amount coverage (₱{fixedAmount:N2}) applied for {targetDescription}: Sponsor pays ₱{fixedAmount:N2}, Parent pays ₱{requestedAmount - fixedAmount:N2}"
                        );
                    }

                case "UpToCap":
                    var capAmount = rule.CapAmount ?? 0;
                    if (capAmount >= requestedAmount)
                    {
                        return (
                            CoverageDecision.Covered,
                            Models.API.BillTo.Sponsor,
                            requestedAmount,
                            0,
                            CoverageReasonCodes.COVERED,
                            "The charge is fully covered by the sponsor."
                        );
                    }
                    else
                    {
                        return (
                            CoverageDecision.Split,
                            Models.API.BillTo.SponsorAndParent,
                            capAmount,
                            requestedAmount - capAmount,
                            CoverageReasonCodes.EXCEEDS_CAP,
                            $"The charge amount exceeds the sponsor coverage cap. The sponsor will cover {capAmount:N2} and the remaining {requestedAmount - capAmount:N2} will be billed to the parent."
                        );
                    }

                default:
                    return (
                        CoverageDecision.NotCovered,
                        Models.API.BillTo.Parent,
                        0,
                        requestedAmount,
                        CoverageReasonCodes.NO_MATCHING_RULE,
                        $"Unknown coverage type: {rule.CoverageType}"
                    );
            }
        }

        private string GenerateRuleVersion(LogCoverage log, LoGCoverageRule rule)
        {
            // Format: LOG{LogId}-RULE{RuleId}-{Timestamp}
            return $"LOG{log.LogId}-RULE{rule.RuleId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        private string SerializeRuleSnapshot(LogCoverage log, LoGCoverageRule rule, CoverageEvaluationRequest request, CoverageEvaluationResponse response)
        {
            try
            {
                // Create a human-readable snapshot with sponsor info
                var readableSnapshot = $"Sponsor {log.SponsorId} {log.Sponsor?.SponsorName}; ";
                
                if (rule.CoverageType == "UpToCap" && rule.CapAmount.HasValue)
                {
                    readableSnapshot += $"sponsor cap: {rule.CapAmount.Value:N2}; charge amount: {request.Amount:N2}; reason: {response.ReasonCode}";
                }
                else
                {
                    readableSnapshot += $"coverage type: {rule.CoverageType}; ";
                    if (rule.CoveragePercentage.HasValue)
                        readableSnapshot += $"percentage: {rule.CoveragePercentage.Value}%; ";
                    if (rule.CoverageFixedAmount.HasValue)
                        readableSnapshot += $"fixed amount: {rule.CoverageFixedAmount.Value:N2}; ";
                    if (rule.CapAmount.HasValue)
                        readableSnapshot += $"cap: {rule.CapAmount.Value:N2}; ";
                    readableSnapshot += $"charge amount: {request.Amount:N2}; reason: {response.ReasonCode}";
                }

                return readableSnapshot;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize rule snapshot for rule {RuleId}", rule.RuleId);
                return string.Empty;
            }
        }

        private void PopulateEvaluationMetadata(CoverageEvaluationRequest request, CoverageEvaluationResponse response)
        {
            // Set timestamp
            response.EvaluatedAt = DateTime.UtcNow;
            
            // Generate or use provided correlation ID for end-to-end traceability
            response.CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString();
            
            // Generate unique decision ID
            response.DecisionId = Guid.NewGuid().ToString();
            
            // Calculate coverage percentages
            if (request.Amount > 0)
            {
                response.SponsorPercent = Math.Round((response.SponsorAmount / request.Amount) * 100, 2);
                response.ParentPercent = Math.Round((response.ParentAmount / request.Amount) * 100, 2);
            }
        }

        private CoverageEvaluationAudit CreateAuditRecord(
            CoverageEvaluationRequest request,
            CoverageEvaluationResponse response,
            string userId,
            string userDisplay,
            string userRole)
        {
            return new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow,
                EvaluatedByUserId = userId,
                EvaluatedByUserDisplay = userDisplay,
                EvaluatedByRole = userRole,
                SchoolYearId = request.SchoolYearId,
                StudentId = request.StudentId,
                SponsorId = request.SponsorId,
                LogId = request.LogId,
                ItemId = request.ItemId,
                CategoryId = request.CategoryId,
                RequestedAmount = request.Amount,
                ChargeDate = request.ChargeDate,
                Decision = response.Decision.ToString(),
                BillTo = response.BillTo.ToString(),
                SponsorAmount = response.SponsorAmount,
                ParentAmount = response.ParentAmount,
                ReasonCode = response.ReasonCode,
                Explanation = response.Explanation,
                MatchedRuleId = response.MatchedRuleId,
                RuleVersion = response.RuleVersion,
                Success = response.Success,
                ErrorMessage = response.ErrorMessage
            };
        }
    }
}
