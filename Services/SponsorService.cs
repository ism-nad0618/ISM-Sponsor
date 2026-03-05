using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class SponsorService
    {
        private readonly AppDbContext _context;
        public SponsorService(AppDbContext context) => _context = context;

        public async Task<Sponsor?> GetByIdAsync(string id) => await _context.Sponsors.Include(s => s.Contacts).FirstOrDefaultAsync(s => s.SponsorId == id);

        public async Task UpdateAsync(Sponsor sponsor)
        {
            _context.Sponsors.Update(sponsor);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SponsorContact>> GetContactsAsync(string sponsorId) =>
            await _context.SponsorContacts.Where(c => c.SponsorId == sponsorId && c.IsActive).ToListAsync();

        public async Task<List<Sponsor>> GetAllAsync() =>
            await _context.Sponsors.OrderBy(s=>s.SponsorName).ToListAsync();

        public async Task AddContactAsync(SponsorContact contact)
        {
            _context.SponsorContacts.Add(contact);
            await _context.SaveChangesAsync();
        }
    }
}