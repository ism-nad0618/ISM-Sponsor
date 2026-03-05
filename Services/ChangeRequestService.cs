using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class ChangeRequestService
    {
        private readonly AppDbContext _context;
        public ChangeRequestService(AppDbContext context) => _context = context;

        public async Task<List<ChangeRequest>> GetPendingAsync(string? sponsorId = null)
        {
            var query = _context.ChangeRequests.Where(cr => cr.Status == "pending");
            if (!string.IsNullOrEmpty(sponsorId))
                query = query.Where(cr => cr.SponsorId == sponsorId);
            return await query.ToListAsync();
        }

        public async Task SubmitAsync(ChangeRequest cr)
        {
            cr.RequestedOn = DateTime.UtcNow;
            _context.ChangeRequests.Add(cr);
            await _context.SaveChangesAsync();
        }

        public async Task ResolveAsync(int id, bool approve, string resolverId)
        {
            var cr = await _context.ChangeRequests.FindAsync(id);
            if (cr == null) return;
            cr.Status = approve ? "approved" : "rejected";
            cr.ResolvedByUserId = resolverId;
            cr.ResolvedOn = DateTime.UtcNow;
            if (approve)
            {
                var sponsor = await _context.Sponsors.FindAsync(cr.SponsorId);
                if (sponsor != null)
                {
                    switch (cr.Field)
                    {
                        case "sponsorName": sponsor.SponsorName = cr.NewValue; break;
                        case "legalName": sponsor.LegalName = cr.NewValue; break;
                        case "address": sponsor.Address = cr.NewValue; break;
                        case "tin": sponsor.Tin = cr.NewValue; break;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}