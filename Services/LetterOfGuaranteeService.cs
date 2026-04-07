using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class LetterOfGuaranteeService
    {
        private readonly AppDbContext _context;
        private readonly LogsService _logsService;

        public LetterOfGuaranteeService(AppDbContext context, LogsService logsService)
        {
            _context = context;
            _logsService = logsService;
        }

        public async Task<List<LogCoverage>> GetAllAsync(string? schoolYear = null, string? sponsorId = null, string? search = null)
        {
            var query = _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(schoolYear))
            {
                query = query.Where(l => l.SchoolYearId == schoolYear);
            }

            if (!string.IsNullOrEmpty(sponsorId))
            {
                query = query.Where(l => l.SponsorId == sponsorId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l =>
                    l.StudentId.Contains(search) ||
                    (l.Student != null && (l.Student.FirstName.Contains(search) || l.Student.LastName.Contains(search))) ||
                    (l.Sponsor != null && l.Sponsor.SponsorName.Contains(search))
                );
            }

            return await query
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();
        }

        public async Task<LogCoverage?> GetByIdAsync(int logId)
        {
            return await _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .Include(l => l.ActivatedByUser)
                .Include(l => l.DeactivatedByUser)
                .Include(l => l.CreatedByUser)
                .Include(l => l.ModifiedByUser)
                .Include(l => l.CoverageRules)
                .Include(l => l.CoverageRules!.Select(r => r.Item))
                .Include(l => l.CoverageRules!.Select(r => r.Category))
                .FirstOrDefaultAsync(l => l.LogId == logId);
        }

        public async Task<LogCoverage> CreateAsync(LogCoverage log, string userId)
        {
            log.CreatedOn = DateTime.UtcNow;
            log.CreatedByUserId = userId;
            log.LogStatus = "Draft";
            log.IsActive = false;

            _context.LogCoverages.Add(log);
           await _context.SaveChangesAsync();

            return log;
        }

        public async Task UpdateAsync(LogCoverage log, string userId)
        {
            log.ModifiedOn = DateTime.UtcNow;
            log.ModifiedByUserId = userId;

            _context.LogCoverages.Update(log);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int logId, string userId, string userDisplay, string roleName)
        {
            var log = await _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .FirstOrDefaultAsync(l => l.LogId == logId);

            if (log == null || log.IsActive)
            {
                return false;
            }

            // Validate that LoG is approved
            if (!log.LogStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only Approved LoGs can be activated");
            }

            log.IsActive = true;
            log.ActivatedOn = DateTime.UtcNow;
            log.ActivatedByUserId = userId;
            log.DeactivatedOn = null;
            log.DeactivatedByUserId = null;
            log.DeactivationReason = null;
            log.ModifiedOn = DateTime.UtcNow;
            log.ModifiedByUserId = userId;

            await _context.SaveChangesAsync();

            // Log activity
            await _logsService.LogActivityAsync(
                item: "LoG Activated",
                details: $"LoG #{log.LogId} activated for Student {log.StudentId} ({log.Student?.FirstName} {log.Student?.LastName}) under Sponsor {log.Sponsor?.SponsorName}",
                userDisplay: userDisplay,
                roleName: roleName,
                schoolYearId: log.SchoolYearId
            );

            return true;
        }

        public async Task<bool> DeactivateAsync(int logId, string reason, string userId, string userDisplay, string roleName)
        {
            var log = await _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .FirstOrDefaultAsync(l => l.LogId == logId);

            if (log == null || !log.IsActive)
            {
                return false;
            }

            log.IsActive = false;
            log.DeactivatedOn = DateTime.UtcNow;
            log.DeactivatedByUserId = userId;
            log.DeactivationReason = reason;
            log.ModifiedOn = DateTime.UtcNow;
            log.ModifiedByUserId = userId;

            await _context.SaveChangesAsync();

            // Log activity
            await _logsService.LogActivityAsync(
                item: "LoG Deactivated",
                details: $"LoG #{log.LogId} deactivated for Student {log.StudentId} ({log.Student?.FirstName} {log.Student?.LastName}). Reason: {reason}",
                userDisplay: userDisplay,
                roleName: roleName,
                schoolYearId: log.SchoolYearId
            );

            return true;
        }

        public async Task<string?> SaveAttachmentAsync(int logId, IFormFile file)
        {
            var log = await _context.LogCoverages.FindAsync(logId);
            if (log == null)
            {
                return null;
            }

            var uploads = Path.Combine("wwwroot", "uploads", "logs", log.SchoolYearId, log.StudentId);
            Directory.CreateDirectory(uploads);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeBaseName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "-")
                .Replace("..", ".");
            var fileName = $"log-{logId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploads, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            log.AttachmentFileName = fileName;
            log.AttachmentUploadedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return fileName;
        }

        public async Task<int> GetActiveLogCountAsync(string? schoolYear = null)
        {
            var query = _context.LogCoverages.Where(l => l.IsActive);

            if (!string.IsNullOrEmpty(schoolYear))
            {
                query = query.Where(l => l.SchoolYearId == schoolYear);
            }

            return await query.CountAsync();
        }

        public async Task<int> GetCoveredStudentsCountAsync(string? schoolYear = null)
        {
            var query = _context.LogCoverages.Where(l => l.IsActive && l.LogStatus == "Approved");

            if (!string.IsNullOrEmpty(schoolYear))
            {
                query = query.Where(l => l.SchoolYearId == schoolYear);
            }

            return await query.Select(l => l.StudentId).Distinct().CountAsync();
        }

        public async Task<bool> LogExistsForStudentAsync(string schoolYearId, string studentId)
        {
            return await _context.LogCoverages
                .AnyAsync(l => l.SchoolYearId == schoolYearId && l.StudentId == studentId);
        }

        public async Task<List<LoGCoverageRule>> GetCoverageRulesAsync(int logId)
        {
            return await _context.LoGCoverageRules
                .Include(r => r.Item)
                .Include(r => r.Category)
                .Where(r => r.LogId == logId)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.RuleId)
                .ToListAsync();
        }

        public async Task AddCoverageRuleAsync(LoGCoverageRule rule, string userId)
        {
            rule.CreatedOn = DateTime.UtcNow;
            rule.CreatedByUserId = userId;

            _context.LoGCoverageRules.Add(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCoverageRuleAsync(LoGCoverageRule rule, string userId)
        {
            rule.ModifiedOn = DateTime.UtcNow;
            rule.ModifiedByUserId = userId;

            _context.LoGCoverageRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCoverageRuleAsync(int ruleId)
        {
            var rule = await _context.LoGCoverageRules.FindAsync(ruleId);
            if (rule != null)
            {
                _context.LoGCoverageRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> ValidateCoverageRules(List<LoGCoverageRule> rules)
        {
            var errors = new List<string>();

            if (rules == null || rules.Count == 0)
            {
                errors.Add("At least one coverage rule must be defined.");
                return errors;
            }

            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                var ruleNum = i + 1;

                // Must have either ItemId or CategoryId
                if (string.IsNullOrEmpty(rule.ItemId) && string.IsNullOrEmpty(rule.CategoryId))
                {
                    errors.Add($"Rule {ruleNum}: Must specify either an Item or a Category.");
                }

                // Coverage type validation
                switch (rule.CoverageType)
                {
                    case "Percentage":
                        if (!rule.CoveragePercentage.HasValue || rule.CoveragePercentage <= 0 || rule.CoveragePercentage > 100)
                            errors.Add($"Rule {ruleNum}: Percentage coverage must be between 0 and 100.");
                        break;
                    case "FixedAmount":
                        if (!rule.CoverageFixedAmount.HasValue || rule.CoverageFixedAmount <= 0)
                            errors.Add($"Rule {ruleNum}: Fixed amount must be greater than zero.");
                        break;
                    case "UpToCap":
                        if (!rule.CapAmount.HasValue || rule.CapAmount <= 0)
                            errors.Add($"Rule {ruleNum}: Cap amount must be greater than zero.");
                        break;
                }

                // Date validation
                if (rule.EffectiveFrom.HasValue && rule.EffectiveTo.HasValue && rule.EffectiveFrom > rule.EffectiveTo)
                {
                    errors.Add($"Rule {ruleNum}: Effective From date must be before Effective To date.");
                }
            }

            return errors;
        }

        public async Task AddCoverageRulesAsync(int logId, List<LoGCoverageRule> rules, string userId)
        {
            foreach (var rule in rules)
            {
                rule.LogId = logId;
                rule.CreatedOn = DateTime.UtcNow;
                rule.CreatedByUserId = userId;
                _context.LoGCoverageRules.Add(rule);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, LogCoverage? Log, List<string> Errors)> CreateLoGWithRulesAsync(
            LogCoverage log, 
            List<LoGCoverageRule>? rules, 
            string userId)
        {
            var errors = new List<string>();

            // Check if LoG already exists
            if (await LogExistsForStudentAsync(log.SchoolYearId, log.StudentId))
            {
                errors.Add("A Letter of Guarantee already exists for this student in the selected school year.");
                return (false, null, errors);
            }

            // Validate coverage rules if provided
            if (rules != null && rules.Any())
            {
                var validationErrors = await ValidateCoverageRules(rules);
                if (validationErrors.Any())
                {
                    return (false, null, validationErrors);
                }
            }

            // Create the LoG
            var createdLog = await CreateAsync(log, userId);

            // Add coverage rules if provided
            if (rules != null && rules.Any())
            {
                await AddCoverageRulesAsync(createdLog.LogId, rules, userId);
            }

            return (true, createdLog, errors);
        }

        public async Task<bool> UpdateLoGWithRulesAsync(
            LogCoverage log,
            List<LoGCoverageRule>? newRules,
            List<int>? rulesToDelete,
            string userId)
        {
            // Validate new rules if provided
            if (newRules != null && newRules.Any())
            {
                var validationErrors = await ValidateCoverageRules(newRules);
                if (validationErrors.Any())
                {
                    return false;
                }
            }

            // Update the LoG
            await UpdateAsync(log, userId);

            // Delete removed rules
            if (rulesToDelete != null && rulesToDelete.Any())
            {
                foreach (var ruleId in rulesToDelete)
                {
                    await DeleteCoverageRuleAsync(ruleId);
                }
            }

            // Add new rules
            if (newRules != null && newRules.Any())
            {
                await AddCoverageRulesAsync(log.LogId, newRules, userId);
            }

            return true;
        }
    }
}
