using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services
{
    public class SchoolYearService
    {
        private readonly AppDbContext _context;
        public SchoolYearService(AppDbContext context) => _context = context;

        public async Task<List<SchoolYear>> GetAllAsync() => await _context.SchoolYears.ToListAsync();

        public async Task<SchoolYear?> GetActiveAsync() => await _context.SchoolYears.FirstOrDefaultAsync(y => y.IsActive);

        public async Task<SchoolYear?> GetByIdAsync(string id) => await _context.SchoolYears.FindAsync(id);

        public async Task<bool> CreateAsync(SchoolYear schoolYear)
        {
            if (await _context.SchoolYears.AnyAsync(y => y.SchoolYearId == schoolYear.SchoolYearId))
                return false;
            
            _context.SchoolYears.Add(schoolYear);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(SchoolYear schoolYear)
        {
            var existing = await _context.SchoolYears.FindAsync(schoolYear.SchoolYearId);
            if (existing == null)
                return false;
            
            existing.Name = schoolYear.Name;
            existing.ValidFrom = schoolYear.ValidFrom;
            existing.ValidTo = schoolYear.ValidTo;
            existing.IsActive = schoolYear.IsActive;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var schoolYear = await _context.SchoolYears.FindAsync(id);
            if (schoolYear == null)
                return false;
            
            // Check if it has related students
            var hasStudents = await _context.Students.AnyAsync(s => s.SchoolYearId == id);
            if (hasStudents)
                return false;
            
            _context.SchoolYears.Remove(schoolYear);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SetActiveAsync(string id)
        {
            var all = await _context.SchoolYears.ToListAsync();
            foreach (var y in all) y.IsActive = y.SchoolYearId == id;
            await _context.SaveChangesAsync();
        }
    }
}