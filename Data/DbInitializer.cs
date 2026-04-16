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
            // ========================================================================
            // PRODUCTION-SAFE DATABASE INITIALIZATION
            // ========================================================================
            // Migrations are NOT run automatically on startup in production.
            // Migrations should be executed via deployment pipeline (azure-pipelines.yml)
            // using: dotnet ef database update --connection "{connection-string}"
            //
            // In Development, migrations can be enabled via:
            // "Database": { "RunMigrationsOnStartup": true }
            //
            // This approach prevents:
            // - Concurrent migration conflicts in scale-out scenarios
            // - Startup delays in production
            // - Unintended schema changes
            // ========================================================================
            
            // Seed roles (idempotent - safe to run multiple times)
            SeedRoles().Wait();
            
            // Seed users (idempotent - only creates if missing)
            SeedUsers().Wait();
            
            // Seed domain data (idempotent)
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
                    new Sponsor { SponsorId = "ACME", SponsorName = "ACME Corp", LegalName = "ACME Corporation", Address = "123 Road", Tin = "123-456", ApprovalStatus = "Approved" },
                    new Sponsor { SponsorId = "XYZBANK", SponsorName = "XYZ Bank", LegalName = "XYZ Banking Co", Address = "456 Street", Tin = "789-012", ApprovalStatus = "Approved" }
                );
                _context.SaveChanges();
            }

            // Update existing sponsors to "Approved" status if ApprovalStatus is null (for existing databases after migration)
            var sponsorsWithoutApprovalStatus = _context.Sponsors.Where(s => s.ApprovalStatus == null).ToList();
            if (sponsorsWithoutApprovalStatus.Any())
            {
                foreach (var sponsor in sponsorsWithoutApprovalStatus)
                {
                    sponsor.ApprovalStatus = "Approved";
                    sponsor.ApprovedOn = sponsor.CreatedOn;
                }
                _context.SaveChanges();
            }

            if (!_context.ItemCategories.Any())
            {
                _context.ItemCategories.AddRange(
                    new ItemCategory { CategoryId = "TUI", CategoryName = "Tuition", Description = "Tuition fees for academic year", IsActive = true },
                    new ItemCategory { CategoryId = "BKS", CategoryName = "Books", Description = "Textbooks and learning materials", IsActive = true },
                    new ItemCategory { CategoryId = "UNI", CategoryName = "Uniforms", Description = "School uniforms and PE attire", IsActive = true },
                    new ItemCategory { CategoryId = "LAB", CategoryName = "Lab Fees", Description = "Science and computer laboratory fees", IsActive = true },
                    new ItemCategory { CategoryId = "ACT", CategoryName = "Activities", Description = "Field trips, events, and extracurriculars", IsActive = true },
                    new ItemCategory { CategoryId = "MIS", CategoryName = "Miscellaneous", Description = "Other school-related expenses", IsActive = true },
                    new ItemCategory { CategoryId = "EDU", CategoryName = "Education Assistance", Description = "Education assistance and sponsorship fees", IsActive = true }
                );
                _context.SaveChanges();
            }
            
            // Add Education Assistance category if it doesn't exist yet (for existing databases)
            if (!_context.ItemCategories.Any(c => c.CategoryId == "EDU"))
            {
                _context.ItemCategories.Add(new ItemCategory 
                { 
                    CategoryId = "EDU", 
                    CategoryName = "Education Assistance", 
                    Description = "Education assistance and sponsorship fees", 
                    IsActive = true 
                });
                _context.SaveChanges();
            }

            if (!_context.Items.Any())
            {
                _context.Items.AddRange(
                    new Item { ItemId = "I1", ItemName = "Item One", GradeLevel = "Grade 1", CategoryId = "TUI", IsActive = true },
                    new Item { ItemId = "I2", ItemName = "Item Two", GradeLevel = "Grade 2", CategoryId = "BKS", IsActive = true }
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

            if (!_context.SponsorChangeRequests.Any())
            {
                // Only seed sample change requests if we have actual users in the system
                // to avoid foreign key violations
                var hasUsers = _context.Users.Any();
                if (!hasUsers)
                {
                    return; // Skip seeding change requests until users are created
                }
                
                // Get first available user for demo data
                var firstUser = _context.Users.FirstOrDefault();
                if (firstUser == null)
                {
                    return;
                }
                
                var baseDate = DateTime.UtcNow.AddDays(-7);
                
                _context.SponsorChangeRequests.AddRange(
                    // Pending requests
                    new SponsorChangeRequest
                    {
                        SponsorId = "ACME",
                        RequestField = "Address",
                        CurrentValue = "123 Road",
                        RequestedValue = "456 New Avenue, Manila",
                        RequestReason = "Company relocated to new office",
                        Status = "Pending",
                        SubmittedByUserId = firstUser.Id,
                        SubmittedByUserDisplay = "ACME Sponsor User",
                        SubmittedOn = baseDate
                    },
                    new SponsorChangeRequest
                    {
                        SponsorId = "XYZBANK",
                        RequestField = "LegalName",
                        CurrentValue = "XYZ Banking Co",
                        RequestedValue = "XYZ Banking Corporation",
                        RequestReason = "Legal entity name change approved by SEC",
                        Status = "Pending",
                        SubmittedByUserId = firstUser.Id,
                        SubmittedByUserDisplay = "XYZ Sponsor User",
                        SubmittedOn = baseDate.AddDays(1)
                    },
                    new SponsorChangeRequest
                    {
                        SponsorId = "ACME",
                        RequestField = "Tin",
                        CurrentValue = "123-456",
                        RequestedValue = "123-456-789",
                        RequestReason = "Correcting TIN format",
                        Status = "Pending",
                        SubmittedByUserId = firstUser.Id,
                        SubmittedByUserDisplay = "ACME Sponsor User",
                        SubmittedOn = baseDate.AddDays(2)
                    },
                    // Approved request
                    new SponsorChangeRequest
                    {
                        SponsorId = "XYZBANK",
                        RequestField = "SponsorName",
                        CurrentValue = "XYZ Bank",
                        RequestedValue = "XYZ Bank Philippines",
                        RequestReason = "Branding update",
                        Status = "Approved",
                        SubmittedByUserId = firstUser.Id,
                        SubmittedByUserDisplay = "XYZ Sponsor User",
                        SubmittedOn = baseDate.AddDays(-2),
                        ReviewedByUserId = firstUser.Id,
                        ReviewedByUserDisplay = "Admin User",
                        ReviewedOn = baseDate.AddDays(-1),
                        ReviewNotes = "Approved for branding consistency"
                    },
                    // Rejected request
                    new SponsorChangeRequest
                    {
                        SponsorId = "ACME",
                        RequestField = "LegalName",
                        CurrentValue = "ACME Corporation",
                        RequestedValue = "ACME Corp",
                        RequestReason = "Simplified name",
                        Status = "Rejected",
                        SubmittedByUserId = firstUser.Id,
                        SubmittedByUserDisplay = "ACME Sponsor User",
                        SubmittedOn = baseDate.AddDays(-5),
                        ReviewedByUserId = firstUser.Id,
                        ReviewedByUserDisplay = "Admin User",
                        ReviewedOn = baseDate.AddDays(-4),
                        ReviewNotes = "Legal name must match SEC registration"
                    }
                );
                _context.SaveChanges();
            }
        }
    }
}
