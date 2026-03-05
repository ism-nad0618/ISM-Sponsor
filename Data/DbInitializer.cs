using ISMSponsor.Models;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Data
{
    public class DbInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(AppDbContext context,
                             UserManager<ApplicationUser> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            _context.Database.Migrate();
            SeedRoles().Wait();
            // Never reseed users during startup: existing ChangeRequests reference AspNetUsers.
            // Startup should be non-destructive for identity data.
            SeedDomainData();
        }

        private async Task SeedRoles()
        {
            var roles = new[] { "admin", "admissions", "cashier", "sponsor" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedUsers()
        {
            async Task ensureUser(string userName, string password, string role, string? sponsorId = null)
            {
                var existing = await _userManager.FindByNameAsync(userName);
                if (existing == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = userName,
                        DisplayName = userName,
                        SponsorId = sponsorId,
                        IsActive = true
                    };

                    var createResult = await _userManager.CreateAsync(newUser, password);
                    if (!createResult.Succeeded)
                    {
                        var createErrors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"Failed to create user {userName}: {createErrors}");
                        return;
                    }

                    existing = newUser;
                }

                // keep seeded users aligned with expected metadata without deleting rows
                var dirty = false;
                if (existing.DisplayName != userName)
                {
                    existing.DisplayName = userName;
                    dirty = true;
                }
                if (existing.SponsorId != sponsorId)
                {
                    existing.SponsorId = sponsorId;
                    dirty = true;
                }
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    dirty = true;
                }

                if (dirty)
                {
                    var updateResult = await _userManager.UpdateAsync(existing);
                    if (!updateResult.Succeeded)
                    {
                        var updateErrors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"Failed to update user {userName}: {updateErrors}");
                    }
                }

                if (!await _userManager.IsInRoleAsync(existing, role))
                {
                    await _userManager.AddToRoleAsync(existing, role);
                }
            }

            await ensureUser("admin", "admin", "admin");
            await ensureUser("admissions", "admissions", "admissions");
            await ensureUser("cashier", "cashier", "cashier");
            await ensureUser("acme_sponsor", "sponsor", "sponsor", "ACME");
            await ensureUser("xyz_sponsor", "sponsor", "sponsor", "XYZBANK");
        }

        private void SeedDomainData()
        {
            if (!_context.SchoolYears.Any())
            {
                _context.SchoolYears.Add(new SchoolYear
                {
                    SchoolYearId = "25-26",
                    Name = "2025-2026",
                    ValidFrom = new DateTime(2025, 6, 1),
                    ValidTo = new DateTime(2026, 5, 31),
                    IsActive = true
                });
                _context.SaveChanges();
            }

            if (!_context.Sponsors.Any())
            {
                _context.Sponsors.AddRange(
                    new Sponsor { SponsorId = "ACME", SponsorName = "ACME Corp", LegalName = "ACME Corporation", Address = "123 Road", Tin = "123-456" },
                    new Sponsor { SponsorId = "XYZBANK", SponsorName = "XYZ Bank", LegalName = "XYZ Banking Co", Address = "456 Street", Tin = "789-012" }
                );
                _context.SaveChanges();
            }

            if (!_context.Items.Any())
            {
                _context.Items.AddRange(
                    new Item { ItemId = "I1", ItemName = "Item One", GradeLevel = "Grade 1", IsActive = true },
                    new Item { ItemId = "I2", ItemName = "Item Two", GradeLevel = "Grade 2", IsActive = true }
                );
                _context.SaveChanges();
            }

            if (!_context.Students.Any())
            {
                _context.Students.AddRange(
                    new Student { SchoolYearId = "25-26", StudentId = "S001", FirstName = "John", LastName = "Doe", GradeLevel = "1", SponsorId = "ACME", StudentStatus = "active" },
                    new Student { SchoolYearId = "25-26", StudentId = "S002", FirstName = "Jane", LastName = "Smith", GradeLevel = "2", SponsorId = "XYZBANK", StudentStatus = "active" }
                );
                _context.SaveChanges();
            }

            if (!_context.LogCoverages.Any())
            {
                _context.LogCoverages.Add(new LogCoverage { SchoolYearId = "25-26", StudentId = "S001", SponsorId = "ACME", LogStatus = "Draft", IsActive = true });
                _context.LogCoverages.Add(new LogCoverage { SchoolYearId = "25-26", StudentId = "S002", SponsorId = "XYZBANK", LogStatus = "Draft", IsActive = true });
                _context.SaveChanges();
            }

            if (!_context.ActivityLogs.Any())
            {
                _context.ActivityLogs.Add(new ActivityLog { Date = DateTime.UtcNow, Item = "Seed", Details = "Initial data", UserDisplay = "system", RoleName = "admin", SchoolYearId = "25-26" });
                _context.SaveChanges();
            }
        }
    }
}
