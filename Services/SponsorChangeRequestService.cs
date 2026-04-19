using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using ISMSponsor.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class SponsorChangeRequestService
    {
        private readonly AppDbContext _context;
        private readonly LogsService _logsService;

        public SponsorChangeRequestService(AppDbContext context, LogsService logsService)
        {
            _context = context;
            _logsService = logsService;
        }

        public async Task<List<SponsorChangeRequest>> GetRequestsBySponsorAsync(string sponsorId)
        {
            return await _context.SponsorChangeRequests
                .Where(r => r.SponsorId == sponsorId)
                .OrderByDescending(r => r.SubmittedOn)
                .ToListAsync();
        }

        public async Task<List<SponsorChangeRequest>> GetPendingRequestsAsync()
        {
            return await _context.SponsorChangeRequests
                .Include(r => r.Sponsor)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.SubmittedOn)
                .ToListAsync();
        }

        public async Task<List<SponsorChangeRequest>> GetAllRequestsAsync(string? status = null, string? sponsorId = null)
        {
            var query = _context.SponsorChangeRequests
                .Include(r => r.Sponsor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(sponsorId))
            {
                query = query.Where(r => r.SponsorId == sponsorId);
            }

            return await query.OrderByDescending(r => r.SubmittedOn).ToListAsync();
        }

        public async Task<SponsorChangeRequest?> GetRequestByIdAsync(int requestId)
        {
            return await _context.SponsorChangeRequests
                .Include(r => r.Sponsor)
                .Include(r => r.SubmittedByUser)
                .Include(r => r.ReviewedByUser)
                .Include(r => r.AppliedByUser)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task<int> GetPendingRequestCountAsync()
        {
            return await _context.SponsorChangeRequests
                .CountAsync(r => r.Status == "Pending");
        }

        public async Task<(bool Success, string Message, int? RequestId)> SubmitRequestAsync(
            SponsorChangeRequestViewModel model,
            string userId,
            string userDisplay)
        {
            // Validate sponsor exists
            var sponsor = await _context.Sponsors.FindAsync(model.SponsorId);
            if (sponsor == null)
            {
                return (false, "Sponsor not found", null);
            }

            // Check if requested value is same as current value
            if (model.CurrentValue == model.RequestedValue)
            {
                return (false, "Requested value is the same as current value", null);
            }

            // Check for duplicate pending request for same field
            var existingPending = await _context.SponsorChangeRequests
                .AnyAsync(r => r.SponsorId == model.SponsorId
                            && r.RequestField == model.RequestField.ToString()
                            && r.Status == "Pending");

            if (existingPending)
            {
                return (false, $"A pending request already exists for {GetFieldDisplayName(model.RequestField)}", null);
            }

            var request = new SponsorChangeRequest
            {
                SponsorId = model.SponsorId,
                RequestField = model.RequestField.ToString(),
                CurrentValue = model.CurrentValue,
                RequestedValue = model.RequestedValue,
                RequestReason = model.RequestReason,
                Status = SponsorRequestStatus.Pending.ToString(),
                SubmittedByUserId = userId,
                SubmittedByUserDisplay = userDisplay,
                SubmittedOn = DateTime.UtcNow
            };

            _context.SponsorChangeRequests.Add(request);
            await _context.SaveChangesAsync();

            // Log activity
            var schoolYear = "25-26";
            await _logsService.LogActivityAsync(
                "SponsorRequest",
                $"Change request submitted for {sponsor.SponsorName}: {GetFieldDisplayName(model.RequestField)}",
                userDisplay,
                "sponsor",
                schoolYear
            );

            return (true, "Request submitted successfully", request.RequestId);
        }

        public async Task<(bool Success, string Message)> ApproveRequestAsync(
            int requestId,
            string reviewNotes,
            string userId,
            string userDisplay)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null)
            {
                return (false, "Request not found");
            }

            if (request.Status != SponsorRequestStatus.Pending.ToString())
            {
                return (false, $"Cannot approve request with status: {request.Status}");
            }

            var sponsor = await _context.Sponsors.FindAsync(request.SponsorId);
            if (sponsor == null)
            {
                return (false, "Sponsor not found");
            }

            // Store old value for audit
            string oldValue = GetSponsorFieldValue(sponsor, request.RequestField);

            // Apply the change to sponsor master data
            bool applied = ApplySponsorFieldChange(sponsor, request.RequestField, request.RequestedValue);
            if (!applied)
            {
                return (false, $"Unable to apply change to field: {request.RequestField}");
            }

            // Update request status to Applied (auto-applied upon approval)
            request.Status = SponsorRequestStatus.Applied.ToString();
            request.ReviewedByUserId = userId;
            request.ReviewedByUserDisplay = userDisplay;
            request.ReviewedOn = DateTime.UtcNow;
            request.ReviewNotes = reviewNotes;
            request.AppliedByUserId = userId;
            request.AppliedByUserDisplay = userDisplay;
            request.AppliedOn = DateTime.UtcNow;
            request.AppliedValue = request.RequestedValue;

            await _context.SaveChangesAsync();

            // Log activity
            var schoolYear1 = "25-26";
            await _logsService.LogActivityAsync(
                "SponsorRequest",
                $"Change request #{requestId} approved and applied for {sponsor.SponsorName}: {request.RequestField}. Changed from '{oldValue}' to '{request.RequestedValue}'",
                userDisplay,
                "admin",
                schoolYear1
            );

            return (true, "Request approved successfully and changes have been applied to the sponsor record.");
        }

        public async Task<(bool Success, string Message)> RejectRequestAsync(
            int requestId,
            string reviewNotes,
            string userId,
            string userDisplay)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null)
            {
                return (false, "Request not found");
            }

            if (request.Status != SponsorRequestStatus.Pending.ToString())
            {
                return (false, $"Cannot reject request with status: {request.Status}");
            }

            request.Status = SponsorRequestStatus.Rejected.ToString();
            request.ReviewedByUserId = userId;
            request.ReviewedByUserDisplay = userDisplay;
            request.ReviewedOn = DateTime.UtcNow;
            request.ReviewNotes = reviewNotes;

            await _context.SaveChangesAsync();

            // Log activity
            var schoolYear2 = "25-26";
            await _logsService.LogActivityAsync(
                "SponsorRequest",
                $"Change request #{requestId} rejected for {request.Sponsor?.SponsorName}: {request.RequestField}",
                userDisplay,
                "admin",
                schoolYear2
            );

            return (true, "Request rejected successfully");
        }

        public async Task<(bool Success, string Message)> ApplyRequestAsync(
            int requestId,
            string userId,
            string userDisplay)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null)
            {
                return (false, "Request not found");
            }

            if (request.Status != SponsorRequestStatus.Approved.ToString())
            {
                return (false, "Only approved requests can be applied");
            }

            var sponsor = await _context.Sponsors.FindAsync(request.SponsorId);
            if (sponsor == null)
            {
                return (false, "Sponsor not found");
            }

            // Store old value for audit
            string oldValue = GetSponsorFieldValue(sponsor, request.RequestField);

            // Apply the change to sponsor master data
            bool applied = ApplySponsorFieldChange(sponsor, request.RequestField, request.RequestedValue);
            if (!applied)
            {
                return (false, $"Unable to apply change to field: {request.RequestField}");
            }

            // Update request status
            request.Status = SponsorRequestStatus.Applied.ToString();
            request.AppliedByUserId = userId;
            request.AppliedByUserDisplay = userDisplay;
            request.AppliedOn = DateTime.UtcNow;
            request.AppliedValue = request.RequestedValue;

            await _context.SaveChangesAsync();

            // Log activity
            var schoolYear3 = "25-26";
            await _logsService.LogActivityAsync(
                "SponsorRequest",
                $"Change request #{requestId} applied for {sponsor.SponsorName}: {request.RequestField}. Changed from '{oldValue}' to '{request.RequestedValue}'",
                userDisplay,
                "admin",
                schoolYear3
            );

            return (true, "Request applied successfully. Sponsor master data has been updated.");
        }

        public async Task<(bool Success, string Message)> CancelRequestAsync(
            int requestId,
            string userId,
            string userDisplay)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null)
            {
                return (false, "Request not found");
            }

            if (request.Status != SponsorRequestStatus.Pending.ToString())
            {
                return (false, $"Cannot cancel request with status: {request.Status}");
            }

            request.Status = SponsorRequestStatus.Cancelled.ToString();
            await _context.SaveChangesAsync();

            // Log activity
            var schoolYear4 = "25-26";
            await _logsService.LogActivityAsync(
                "SponsorRequest",
                $"Change request #{requestId} cancelled for {request.Sponsor?.SponsorName}: {request.RequestField}",
                userDisplay,
                "sponsor",
                schoolYear4
            );

            return (true, "Request cancelled successfully");
        }

        private string GetSponsorFieldValue(Sponsor sponsor, string fieldName)
        {
            return fieldName switch
            {
                "SponsorName" => sponsor.SponsorName ?? string.Empty,
                "LegalName" => sponsor.LegalName ?? string.Empty,
                "Address" => sponsor.Address ?? string.Empty,
                "Tin" => sponsor.Tin ?? string.Empty,
                _ => string.Empty
            };
        }

        private bool ApplySponsorFieldChange(Sponsor sponsor, string fieldName, string newValue)
        {
            switch (fieldName)
            {
                case "SponsorName":
                    sponsor.SponsorName = newValue;
                    return true;
                case "LegalName":
                    sponsor.LegalName = newValue;
                    return true;
                case "Address":
                    sponsor.Address = newValue;
                    return true;
                case "Tin":
                    sponsor.Tin = newValue;
                    return true;
                default:
                    return false;
            }
        }

        private string GetFieldDisplayName(SponsorRequestField field)
        {
            return field switch
            {
                SponsorRequestField.SponsorName => "Sponsor Name",
                SponsorRequestField.LegalName => "Legal Name",
                SponsorRequestField.Address => "Address",
                SponsorRequestField.Tin => "TIN",
                SponsorRequestField.ContactName => "Contact Name",
                SponsorRequestField.ContactEmail => "Contact Email",
                SponsorRequestField.ContactPhone => "Contact Phone",
                _ => field.ToString()
            };
        }

        public string GetFieldDisplayName(string field)
        {
            if (Enum.TryParse<SponsorRequestField>(field, out var fieldEnum))
            {
                return GetFieldDisplayName(fieldEnum);
            }
            return field;
        }
    }
}
