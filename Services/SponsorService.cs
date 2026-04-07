using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Http;
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
            await _context.SponsorContacts.Where(c => c.SponsorId == sponsorId).OrderByDescending(c => c.IsActive).ThenBy(c => c.Name).ToListAsync();

        public async Task<List<SponsorContact>> GetAllContactsAsync() =>
            await _context.SponsorContacts.Include(c => c.Sponsor).OrderBy(c => c.Name).ToListAsync();

        public async Task<List<Sponsor>> GetAllAsync() =>
            await _context.Sponsors.OrderBy(s=>s.SponsorName).ToListAsync();

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();
            return sponsor;
        }

        public async Task AddContactAsync(SponsorContact contact)
        {
            _context.SponsorContacts.Add(contact);
            await _context.SaveChangesAsync();
        }

        public async Task<SponsorContact?> GetContactByIdAsync(int contactId)
        {
            return await _context.SponsorContacts.FindAsync(contactId);
        }

        public async Task<bool> UpdateContactAsync(int contactId, string name, string email, string phone)
        {
            var contact = await _context.SponsorContacts.FindAsync(contactId);
            if (contact == null) return false;

            contact.Name = name;
            contact.Email = email;
            contact.Phone = phone;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetContactStatusAsync(int contactId, bool isActive)
        {
            var contact = await _context.SponsorContacts.FindAsync(contactId);
            if (contact == null) return false;

            contact.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SponsorExistsAsync(string sponsorId)
        {
            return await _context.Sponsors.AnyAsync(s => s.SponsorId == sponsorId);
        }

        public async Task<string> SaveVerificationDocumentAsync(string sponsorId, IFormFile file)
        {
            var uploads = Path.Combine("wwwroot", "uploads", "sponsors", sponsorId);
            Directory.CreateDirectory(uploads);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeBaseName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "-")
                .Replace("..", ".");
            var fileName = $"verification-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploads, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}