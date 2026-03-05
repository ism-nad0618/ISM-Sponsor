using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class LogsService
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg"];
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;
        private const long MinImageFileSizeBytes = 30 * 1024;

        private readonly AppDbContext _context;

        public LogsService(AppDbContext context) => _context = context;

        public async Task<List<LogCoverage>> GetByYearAsync(string year, string? sponsorId = null)
        {
            var query = _context.LogCoverages.Where(l => l.SchoolYearId == year);
            if (!string.IsNullOrEmpty(sponsorId))
            {
                query = query.Where(l => l.SponsorId == sponsorId);
            }
            return await query.OrderBy(l => l.StudentId).ToListAsync();
        }

        public async Task<List<ActivityLog>> GetRecentActivityAsync(string year, int take = 20)
        {
            return await _context.ActivityLogs
                .Where(a => a.SchoolYearId == year)
                .OrderByDescending(a => a.Date)
                .Take(take)
                .ToListAsync();
        }

        public async Task UploadCoverageAsync(string year, string studentId, IFormFile file, string userDisplay, string roleName)
        {
            ValidateUpload(file);

            var log = await _context.LogCoverages.FindAsync(year, studentId);
            if (log == null)
            {
                return;
            }

            var uploads = Path.Combine("wwwroot", "uploads", "logs", year, studentId);
            Directory.CreateDirectory(uploads);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeBaseName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "-")
                .Replace("..", ".");
            var fileName = $"{safeBaseName}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploads, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            log.AttachmentFileName = fileName;
            log.AttachmentUploadedOn = DateTime.UtcNow;
            if (log.LogStatus.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                log.LogStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                log.LogStatus = "Submitted";
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                Date = DateTime.UtcNow,
                Item = "LoGs Upload",
                Details = $"Student {studentId} uploaded file {fileName}.",
                UserDisplay = userDisplay,
                RoleName = roleName,
                SchoolYearId = year
            });

            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(
            string schoolYearId,
            string studentId,
            string status,
            string? comment,
            string userDisplay,
            string roleName)
        {
            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Submitted", "UnderReview", "Approved", "Rejected"
            };

            if (!allowedStatuses.Contains(status))
            {
                throw new InvalidOperationException("Invalid status value.");
            }

            if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(comment))
            {
                throw new InvalidOperationException("A rejection comment is required.");
            }

            var log = await _context.LogCoverages.FindAsync(schoolYearId, studentId);
            if (log == null)
            {
                throw new InvalidOperationException("LoGs record not found.");
            }

            var previousStatus = log.LogStatus;
            log.LogStatus = status;

            _context.ActivityLogs.Add(new ActivityLog
            {
                Date = DateTime.UtcNow,
                Item = "LoGs Status Update",
                Details = $"Student {studentId}: {previousStatus} -> {status}. Comment: {(string.IsNullOrWhiteSpace(comment) ? "N/A" : comment.Trim())}",
                UserDisplay = userDisplay,
                RoleName = roleName,
                SchoolYearId = schoolYearId
            });

            await _context.SaveChangesAsync();
        }

        private static void ValidateUpload(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException("Invalid file type. Upload PDF, JPG, JPEG, or PNG files only.");
            }

            if (file.Length <= 0 || file.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("File size must be greater than 0 and no more than 10 MB.");
            }

            if ((ext == ".jpg" || ext == ".jpeg" || ext == ".png") && file.Length < MinImageFileSizeBytes)
            {
                throw new InvalidOperationException("Image quality check failed. Upload an image larger than 30 KB.");
            }
        }
    }
}
