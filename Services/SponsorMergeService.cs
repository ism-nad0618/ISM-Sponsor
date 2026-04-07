using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ISMSponsor.Services;

/// <summary>
/// Service for executing controlled sponsor merge operations with full audit trail.
/// Ensures transactional integrity and child record reassignment.
/// </summary>
public class SponsorMergeService
{
    private readonly AppDbContext _context;

    public SponsorMergeService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Previews merge impact without executing.
    /// Returns counts of affected records.
    /// </summary>
    public async Task<MergePreview> PreviewMergeAsync(string survivingSponsorId, string mergedSponsorId)
    {
        var survivor = await _context.Sponsors
            .Include(s => s.Addresses)
            .Include(s => s.Contacts)
            .Include(s => s.Students)
            .FirstOrDefaultAsync(s => s.SponsorId == survivingSponsorId);

        var merged = await _context.Sponsors
            .Include(s => s.Addresses)
            .Include(s => s.Contacts)
            .Include(s => s.Students)
            .FirstOrDefaultAsync(s => s.SponsorId == mergedSponsorId);

        if (survivor == null || merged == null)
        {
            return new MergePreview
            {
                IsValid = false,
                ValidationMessage = "One or both sponsors not found"
            };
        }

        if (survivor.IsMerged || merged.IsMerged)
        {
            return new MergePreview
            {
                IsValid = false,
                ValidationMessage = "Cannot merge sponsors that have already been merged"
            };
        }

        if (survivingSponsorId == mergedSponsorId)
        {
            return new MergePreview
            {
                IsValid = false,
                ValidationMessage = "Cannot merge a sponsor with itself"
            };
        }

        // Count child records
        var addressCount = await _context.SponsorAddresses
            .CountAsync(a => a.SponsorId == mergedSponsorId);

        var contactCount = await _context.SponsorContacts
            .CountAsync(c => c.SponsorId == mergedSponsorId);

        var studentCount = await _context.Students
            .CountAsync(s => s.SponsorId == mergedSponsorId);

        var logCount = await _context.LogCoverages
            .CountAsync(l => l.SponsorId == mergedSponsorId);

        var changeRequestCount = await _context.SponsorChangeRequests
            .CountAsync(cr => cr.SponsorId == mergedSponsorId);

        var userCount = await _context.Users
            .CountAsync(u => u.SponsorId == mergedSponsorId);

        return new MergePreview
        {
            IsValid = true,
            SurvivingSponsor = survivor,
            MergedSponsor = merged,
            AddressesToReassign = addressCount,
            ContactsToReassign = contactCount,
            StudentsToReassign = studentCount,
            LogsToReassign = logCount,
            ChangeRequestsToReassign = changeRequestCount,
            UsersToReassign = userCount,
            TotalRecordsAffected = addressCount + contactCount + studentCount + logCount + changeRequestCount + userCount
        };
    }

    /// <summary>
    /// Executes merge operation transactionally with full audit trail.
    /// Returns the completed MergeOperation record or null if failed.
    /// </summary>
    public async Task<MergeOperation?> ExecuteMergeAsync(
        string survivingSponsorId,
        string mergedSponsorId,
        string initiatedByUserId,
        string mergeReason,
        Dictionary<string, string>? fieldSelections = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var preview = await PreviewMergeAsync(survivingSponsorId, mergedSponsorId);
            if (!preview.IsValid)
            {
                return null;
            }

            var survivor = preview.SurvivingSponsor!;
            var merged = preview.MergedSponsor!;

            // Create merge operation record
            var mergeOp = new MergeOperation
            {
                SurvivingSponsorId = survivingSponsorId,
                MergedSponsorId = mergedSponsorId,
                Status = "InProgress",
                InitiatedOn = DateTime.UtcNow,
                InitiatedByUserId = initiatedByUserId,
                MergeReason = mergeReason,
                FieldSelections = fieldSelections != null ? JsonSerializer.Serialize(fieldSelections) : null,
                SurvivorBeforeSnapshot = JsonSerializer.Serialize(new
                {
                    sponsor = survivor,
                    addressCount = survivor.Addresses?.Count ?? 0,
                    contactCount = survivor.Contacts?.Count ?? 0,
                    studentCount = survivor.Students?.Count ?? 0
                }),
                MergedSponsorSnapshot = JsonSerializer.Serialize(new
                {
                    sponsor = merged,
                    addressCount = merged.Addresses?.Count ?? 0,
                    contactCount = merged.Contacts?.Count ?? 0,
                    studentCount = merged.Students?.Count ?? 0
                })
            };

            _context.MergeOperations.Add(mergeOp);
            await _context.SaveChangesAsync();

            // Apply field selections if provided (e.g., prefer merged sponsor's legal name)
            if (fieldSelections != null)
            {
                foreach (var (field, source) in fieldSelections)
                {
                    if (source == "merged")
                    {
                        // Copy selected fields from merged to survivor
                        switch (field)
                        {
                            case "LegalName":
                                survivor.LegalName = merged.LegalName;
                                break;
                            case "Tin":
                                survivor.Tin = merged.Tin;
                                break;
                            case "Address":
                                survivor.Address = merged.Address;
                                break;
                        }
                    }
                }
            }

            // Preserve external IDs from merged record if survivor doesn't have them
            if (string.IsNullOrWhiteSpace(survivor.PowerSchoolId) && !string.IsNullOrWhiteSpace(merged.PowerSchoolId))
                survivor.PowerSchoolId = merged.PowerSchoolId;

            if (string.IsNullOrWhiteSpace(survivor.NetSuiteId) && !string.IsNullOrWhiteSpace(merged.NetSuiteId))
                survivor.NetSuiteId = merged.NetSuiteId;

            if (string.IsNullOrWhiteSpace(survivor.StudentChargingPortalId) && !string.IsNullOrWhiteSpace(merged.StudentChargingPortalId))
                survivor.StudentChargingPortalId = merged.StudentChargingPortalId;

            if (string.IsNullOrWhiteSpace(survivor.OnlineBillingSystemId) && !string.IsNullOrWhiteSpace(merged.OnlineBillingSystemId))
                survivor.OnlineBillingSystemId = merged.OnlineBillingSystemId;

            if (string.IsNullOrWhiteSpace(survivor.ExternalSystemId) && !string.IsNullOrWhiteSpace(merged.ExternalSystemId))
                survivor.ExternalSystemId = merged.ExternalSystemId;

            survivor.ModifiedOn = DateTime.UtcNow;

            // Reassign child records
            var addressesReassigned = await ReassignAddressesAsync(mergedSponsorId, survivingSponsorId);
            var contactsReassigned = await ReassignContactsAsync(mergedSponsorId, survivingSponsorId);
            var studentsReassigned = await ReassignStudentsAsync(mergedSponsorId, survivingSponsorId);
            var logsReassigned = await ReassignLoGsAsync(mergedSponsorId, survivingSponsorId);
            var requestsReassigned = await ReassignChangeRequestsAsync(mergedSponsorId, survivingSponsorId);
            var usersReassigned = await ReassignUsersAsync(mergedSponsorId, survivingSponsorId);

            // Update merged sponsor to mark as merged
            merged.IsActive = false;
            merged.IsMerged = true;
            merged.MergedIntoSponsorId = survivingSponsorId;
            merged.MergedOn = DateTime.UtcNow;
            merged.MergeOperationId = mergeOp.MergeOperationId;
            merged.ModifiedOn = DateTime.UtcNow;

            // Update merge operation with results
            mergeOp.Status = "Completed";
            mergeOp.CompletedOn = DateTime.UtcNow;
            mergeOp.ChildRecordsReassigned = addressesReassigned + contactsReassigned + studentsReassigned + logsReassigned;
            mergeOp.UsersReassigned = usersReassigned;
            mergeOp.LogsReassigned = logsReassigned;
            mergeOp.RequestsReassigned = requestsReassigned;

            // Update any duplicate candidates involving these sponsors
            var candidates = await _context.SponsorDuplicateCandidates
                .Where(c => (c.PrimarySponsorId == mergedSponsorId || c.DuplicateSponsorId == mergedSponsorId) &&
                           c.Status != "Merged")
                .ToListAsync();

            foreach (var candidate in candidates)
            {
                candidate.Status = "Merged";
                candidate.MergeOperationId = mergeOp.MergeOperationId;
            }

            // Create activity log
            var activityLog = new ActivityLog
            {
                Date = DateTime.UtcNow,
                Item = "Sponsor Merge",
                Details = $"Merged sponsor '{merged.SponsorName}' ({mergedSponsorId}) into '{survivor.SponsorName}' ({survivingSponsorId}). Reassigned {mergeOp.ChildRecordsReassigned} records.",
                UserDisplay = initiatedByUserId,
                RoleName = "admin",
                SchoolYearId = string.Empty
            };
            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return mergeOp;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            // Log the failure
            var failedOp = new MergeOperation
            {
                SurvivingSponsorId = survivingSponsorId,
                MergedSponsorId = mergedSponsorId,
                Status = "Failed",
                InitiatedOn = DateTime.UtcNow,
                InitiatedByUserId = initiatedByUserId,
                MergeReason = mergeReason,
                CompletedOn = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };

            _context.MergeOperations.Add(failedOp);
            await _context.SaveChangesAsync();

            return null;
        }
    }

    private async Task<int> ReassignAddressesAsync(string fromSponsorId, string toSponsorId)
    {
        var addresses = await _context.SponsorAddresses
            .Where(a => a.SponsorId == fromSponsorId)
            .ToListAsync();

        foreach (var address in addresses)
        {
            address.SponsorId = toSponsorId;
        }

        return addresses.Count;
    }

    private async Task<int> ReassignContactsAsync(string fromSponsorId, string toSponsorId)
    {
        var contacts = await _context.SponsorContacts
            .Where(c => c.SponsorId == fromSponsorId)
            .ToListAsync();

        foreach (var contact in contacts)
        {
            contact.SponsorId = toSponsorId;
        }

        return contacts.Count;
    }

    private async Task<int> ReassignStudentsAsync(string fromSponsorId, string toSponsorId)
    {
        var students = await _context.Students
            .Where(s => s.SponsorId == fromSponsorId)
            .ToListAsync();

        foreach (var student in students)
        {
            student.SponsorId = toSponsorId;
        }

        return students.Count;
    }

    private async Task<int> ReassignLoGsAsync(string fromSponsorId, string toSponsorId)
    {
        var logs = await _context.LogCoverages
            .Where(l => l.SponsorId == fromSponsorId)
            .ToListAsync();

        foreach (var log in logs)
        {
            log.SponsorId = toSponsorId;
        }

        return logs.Count;
    }

    private async Task<int> ReassignChangeRequestsAsync(string fromSponsorId, string toSponsorId)
    {
        var requests = await _context.SponsorChangeRequests
            .Where(cr => cr.SponsorId == fromSponsorId)
            .ToListAsync();

        foreach (var request in requests)
        {
            request.SponsorId = toSponsorId;
        }

        return requests.Count;
    }

    private async Task<int> ReassignUsersAsync(string fromSponsorId, string toSponsorId)
    {
        var users = await _context.Users
            .Where(u => u.SponsorId == fromSponsorId)
            .ToListAsync();

        foreach (var user in users)
        {
            user.SponsorId = toSponsorId;
        }

        return users.Count;
    }

    /// <summary>
    /// Gets merge operation history for a sponsor (both as survivor and merged).
    /// </summary>
    public async Task<List<MergeOperation>> GetSponsorMergeHistoryAsync(string sponsorId)
    {
        return await _context.MergeOperations
            .Where(m => m.SurvivingSponsorId == sponsorId || m.MergedSponsorId == sponsorId)
            .OrderByDescending(m => m.InitiatedOn)
            .ToListAsync();
    }
}

/// <summary>
/// Preview model for merge impact assessment
/// </summary>
public class MergePreview
{
    public bool IsValid { get; set; }
    public string? ValidationMessage { get; set; }
    public Sponsor? SurvivingSponsor { get; set; }
    public Sponsor? MergedSponsor { get; set; }
    public int AddressesToReassign { get; set; }
    public int ContactsToReassign { get; set; }
    public int StudentsToReassign { get; set; }
    public int LogsToReassign { get; set; }
    public int ChangeRequestsToReassign { get; set; }
    public int UsersToReassign { get; set; }
    public int TotalRecordsAffected { get; set; }
}
