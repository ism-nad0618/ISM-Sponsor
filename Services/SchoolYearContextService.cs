using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services;

/// <summary>
/// Manages the selected school year context throughout the application session.
/// The selected school year affects dashboard metrics, LoG lists, and other views.
/// </summary>
public class SchoolYearContextService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionKey = "SelectedSchoolYearId";

    public SchoolYearContextService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the currently selected school year ID from session.
    /// If none is selected, returns the active school year ID.
    /// </summary>
    public async Task<string> GetSelectedSchoolYearIdAsync()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var selectedId = session?.GetString(SessionKey);

        if (!string.IsNullOrEmpty(selectedId))
        {
            // Verify it still exists in database
            var exists = await _context.SchoolYears
                .AnyAsync(sy => sy.SchoolYearId == selectedId);
            if (exists)
            {
                return selectedId;
            }
        }

        // Fall back to active school year
        var activeYear = await _context.SchoolYears
            .Where(sy => sy.IsActive)
            .OrderByDescending(sy => sy.ValidFrom)
            .FirstOrDefaultAsync();

        if (activeYear != null)
        {
            return activeYear.SchoolYearId;
        }

        // Last resort: any school year
        var anyYear = await _context.SchoolYears
            .OrderByDescending(sy => sy.ValidFrom)
            .FirstOrDefaultAsync();

        return anyYear?.SchoolYearId ?? string.Empty;
    }

    /// <summary>
    /// Gets the currently selected school year entity.
    /// </summary>
    public async Task<SchoolYear?> GetSelectedSchoolYearAsync()
    {
        var id = await GetSelectedSchoolYearIdAsync();
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return await _context.SchoolYears
            .FirstOrDefaultAsync(sy => sy.SchoolYearId == id);
    }

    /// <summary>
    /// Sets the selected school year ID in session.
    /// </summary>
    public void SetSelectedSchoolYearId(string schoolYearId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null && !string.IsNullOrEmpty(schoolYearId))
        {
            session.SetString(SessionKey, schoolYearId);
        }
    }

    /// <summary>
    /// Gets all available school years for the selector dropdown.
    /// </summary>
    public async Task<List<SchoolYear>> GetAllSchoolYearsAsync()
    {
        return await _context.SchoolYears
            .OrderByDescending(sy => sy.ValidFrom)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the active school year.
    /// </summary>
    public async Task<SchoolYear?> GetActiveSchoolYearAsync()
    {
        return await _context.SchoolYears
            .Where(sy => sy.IsActive)
            .OrderByDescending(sy => sy.ValidFrom)
            .FirstOrDefaultAsync();
    }
}
