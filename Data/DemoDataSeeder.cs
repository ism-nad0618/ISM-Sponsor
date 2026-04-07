using ISMSponsor.Data;
using ISMSponsor.Models;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Data;

public class DemoDataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DemoDataSeeder(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedDemoDataAsync()
    {
        // Check if demo data already exists
        if (await _context.Sponsors.AnyAsync(s => s.SponsorId.StartsWith("DEMO-")))
        {
            Console.WriteLine("Demo data already exists. Skipping seed.");
            return;
        }

        Console.WriteLine("Starting demo data seed...");

        // 1. Seed demo users (in addition to existing admin)
        await SeedDemoUsersAsync();

        // 2. Seed school years
        await SeedSchoolYearsAsync();

        // 3. Seed items (fee codes)
        await SeedItemsAsync();

        // 4. Seed demo sponsors
        await SeedDemoSponsorsAsync();

        // 5. Seed demo students
        await SeedDemoStudentsAsync();

        // 6. Seed demo LoGs
        await SeedDemoLoGsAsync();

        // 7. Seed demo change requests
        await SeedDemoChangeRequestsAsync();

        // 8. Seed demo sync logs
        await SeedDemoSyncLogsAsync();

        // 9. Seed demo audit logs
        await SeedDemoAuditLogsAsync();

        // 10. Seed demo coverage evaluations
        await SeedDemoCoverageEvaluationsAsync();

        await _context.SaveChangesAsync();
        Console.WriteLine("Demo data seed completed successfully!");
    }

    private async Task SeedDemoUsersAsync()
    {
        // Create demo users for each role
        var demoUsers = new[]
        {
            new { Email = "demo.admin@ism.edu.ph", Role = "Admin", DisplayName = "Alex Administrator" },
            new { Email = "demo.admissions@ism.edu.ph", Role = "Admissions", DisplayName = "Amy Admissions" },
            new { Email = "demo.cashier@ism.edu.ph", Role = "Cashier", DisplayName = "Carlos Cashier" },
            new { Email = "demo.sponsor@ism.edu.ph", Role = "Sponsor", DisplayName = "Sarah Sponsor" }
        };

        foreach (var userData in demoUsers)
        {
            var existingUser = await _userManager.FindByEmailAsync(userData.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    EmailConfirmed = true,
                    DisplayName = userData.DisplayName,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, "Demo@2026!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, userData.Role);
                    Console.WriteLine($"Created demo user: {userData.Email} with role {userData.Role}");
                }
            }
        }
    }

    private async Task SeedSchoolYearsAsync()
    {
        var schoolYears = new[]
        {
            new SchoolYear { SchoolYearId = "24-25", Name = "2024-2025", ValidFrom = new DateTime(2024, 8, 1), ValidTo = new DateTime(2025, 6, 30), IsActive = false },
            new SchoolYear { SchoolYearId = "25-26", Name = "2025-2026", ValidFrom = new DateTime(2025, 8, 1), ValidTo = new DateTime(2026, 6, 30), IsActive = true },
            new SchoolYear { SchoolYearId = "26-27", Name = "2026-2027", ValidFrom = new DateTime(2026, 8, 1), ValidTo = new DateTime(2027, 6, 30), IsActive = false }
        };

        foreach (var sy in schoolYears)
        {
            if (!await _context.SchoolYears.AnyAsync(s => s.SchoolYearId == sy.SchoolYearId))
            {
                _context.SchoolYears.Add(sy);
            }
        }
    }

    private async Task SeedItemsAsync()
    {
        // First seed categories
        var categories = new[]
        {
            new ItemCategory { CategoryId = "TUITION", CategoryName = "Tuition", Description = "Tuition fees", IsActive = true },
            new ItemCategory { CategoryId = "TRANSPORT", CategoryName = "Transportation", Description = "Transportation fees", IsActive = true },
            new ItemCategory { CategoryId = "FOOD", CategoryName = "Food", Description = "Food service fees", IsActive = true },
            new ItemCategory { CategoryId = "ACTIVITY", CategoryName = "Activity", Description = "Activity fees", IsActive = true },
            new ItemCategory { CategoryId = "FEE", CategoryName = "Fee", Description = "Miscellaneous fees", IsActive = true },
            new ItemCategory { CategoryId = "MATERIAL", CategoryName = "Material", Description = "Materials and supplies", IsActive = true }
        };

        foreach (var category in categories)
        {
            if (!await _context.Set<ItemCategory>().AnyAsync(c => c.CategoryId == category.CategoryId))
            {
                _context.Set<ItemCategory>().Add(category);
            }
        }
        await _context.SaveChangesAsync();

        var items = new[]
        {
            new Item { ItemId = "TUITION-ES", ItemName = "Elementary School Tuition", GradeLevel = "ES", CategoryId = "TUITION", IsActive = true },
            new Item { ItemId = "TUITION-MS", ItemName = "Middle School Tuition", GradeLevel = "MS", CategoryId = "TUITION", IsActive = true },
            new Item { ItemId = "TUITION-HS", ItemName = "High School Tuition", GradeLevel = "HS", CategoryId = "TUITION", IsActive = true },
            new Item { ItemId = "BUS-SERVICE", ItemName = "Bus Transportation", GradeLevel = "ALL", CategoryId = "TRANSPORT", IsActive = true },
            new Item { ItemId = "LUNCH-PLAN", ItemName = "Lunch Plan", GradeLevel = "ALL", CategoryId = "FOOD", IsActive = true },
            new Item { ItemId = "FIELD-TRIP", ItemName = "Field Trip Fee", GradeLevel = "ALL", CategoryId = "ACTIVITY", IsActive = true },
            new Item { ItemId = "TECHNOLOGY", ItemName = "Technology Fee", GradeLevel = "ALL", CategoryId = "FEE", IsActive = true },
            new Item { ItemId = "UNIFORM", ItemName = "School Uniform", GradeLevel = "ALL", CategoryId = "MATERIAL", IsActive = true },
            new Item { ItemId = "TEXTBOOK", ItemName = "Textbook Fee", GradeLevel = "ALL", CategoryId = "MATERIAL", IsActive = true },
            new Item { ItemId = "LAB-FEE", ItemName = "Laboratory Fee", GradeLevel = "HS", CategoryId = "FEE", IsActive = true }
        };

        foreach (var item in items)
        {
            if (!await _context.Items.AnyAsync(i => i.ItemId == item.ItemId))
            {
                _context.Items.Add(item);
            }
        }
    }

    private async Task SeedDemoSponsorsAsync()
    {
        var adminUser = await _userManager.FindByEmailAsync("demo.admin@ism.edu.ph");
        var adminUserId = adminUser?.Id ?? "system";

        // DEMO-SP001: Global Tech Corporation
        if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == "DEMO-SP001"))
        {
            var sponsor = new Sponsor
            {
                SponsorId = "DEMO-SP001",
                SponsorName = "Global Tech Corporation",
                LegalName = "Global Tech Corporation Philippines Inc.",
                Address = "25th Floor, Corporate Tower, Bonifacio Global City, Taguig",
                Tin = "123-456-789-000",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-6),
                CreatedByUserId = adminUserId,
                PowerSchoolId = "PS001",
                NetSuiteId = "NS001"
            };
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _context.Set<SponsorContact>().Add(new SponsorContact
            {
                SponsorId = "DEMO-SP001",
                Name = "Maria Santos",
                Email = "maria.santos@globaltech.com",
                Phone = "+63 2 8123 4567",
                IsActive = true
            });

            _context.Set<SponsorAddress>().Add(new SponsorAddress
            {
                SponsorId = "DEMO-SP001",
                AddressType = "Billing",
                AddressLine1 = "25th Floor, Corporate Tower",
                AddressLine2 = "Bonifacio Global City",
                City = "Taguig",
                StateProvince = "Metro Manila",
                PostalCode = "1634",
                Country = "Philippines",
                IsPrimary = true,
                IsActive = true
            });
        }

        // DEMO-SP002: Asian Development Bank
        if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == "DEMO-SP002"))
        {
            var sponsor = new Sponsor
            {
                SponsorId = "DEMO-SP002",
                SponsorName = "Asian Development Bank",
                LegalName = "Asian Development Bank",
                Address = "6 ADB Avenue, Mandaluyong City",
                Tin = "234-567-890-000",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-5),
                CreatedByUserId = adminUserId,
                PowerSchoolId = "PS002"
            };
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _context.Set<SponsorContact>().Add(new SponsorContact
            {
                SponsorId = "DEMO-SP002",
                Name = "John Lee",
                Email = "j.lee@adb.org",
                Phone = "+63 2 8632 4444",
                IsActive = true
            });

            _context.Set<SponsorAddress>().Add(new SponsorAddress
            {
                SponsorId = "DEMO-SP002",
                AddressType = "Billing",
                AddressLine1 = "6 ADB Avenue",
                AddressLine2 = "Mandaluyong City",
                City = "Mandaluyong",
                StateProvince = "Metro Manila",
                PostalCode = "1550",
                Country = "Philippines",
                IsPrimary = true,
                IsActive = true
            });
        }

        // DEMO-SP003: Embassy of Canada
        if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == "DEMO-SP003"))
        {
            var sponsor = new Sponsor
            {
                SponsorId = "DEMO-SP003",
                SponsorName = "Embassy of Canada",
                LegalName = "Embassy of Canada to the Philippines",
                Address = "Tower 2, RCBC Plaza, 6819 Ayala Avenue, Makati",
                Tin = "345-678-901-000",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-4),
                CreatedByUserId = adminUserId,
                NetSuiteId = "NS003"
            };
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _context.Set<SponsorContact>().Add(new SponsorContact
            {
                SponsorId = "DEMO-SP003",
                Name = "Robert Johnson",
                Email = "robert.johnson@canada.ca",
                Phone = "+63 2 8857 9000",
                IsActive = true
            });

            _context.Set<SponsorAddress>().Add(new SponsorAddress
            {
                SponsorId = "DEMO-SP003",
                AddressType = "Billing",
                AddressLine1 = "Tower 2, RCBC Plaza",
                AddressLine2 = "6819 Ayala Avenue",
                City = "Makati",
                StateProvince = "Metro Manila",
                PostalCode = "1200",
                Country = "Philippines",
                IsPrimary = true,
                IsActive = true
            });
        }

        // DEMO-SP004: Manila Consulting Group
        if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == "DEMO-SP004"))
        {
            var sponsor = new Sponsor
            {
                SponsorId = "DEMO-SP004",
                SponsorName = "Manila Consulting Group",
                LegalName = "Manila Consulting Group Inc.",
                Address = "15th Floor, Zuellig Building, Makati Avenue, Makati",
                Tin = "456-789-012-000",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-3),
                CreatedByUserId = adminUserId
            };
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _context.Set<SponsorContact>().Add(new SponsorContact
            {
                SponsorId = "DEMO-SP004",
                Name = "Ana Reyes",
                Email = "ana.reyes@manilacons.com",
                Phone = "+63 2 8812 3456",
                IsActive = true
            });

            _context.Set<SponsorAddress>().Add(new SponsorAddress
            {
                SponsorId = "DEMO-SP004",
                AddressType = "Billing",
                AddressLine1 = "15th Floor, Zuellig Building",
                AddressLine2 = "Makati Avenue",
                City = "Makati",
                StateProvince = "Metro Manila",
                PostalCode = "1227",
                Country = "Philippines",
                IsPrimary = true,
                IsActive = true
            });
        }

        // DEMO-SP005: Pacific Resources Ltd (inactive)
        if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == "DEMO-SP005"))
        {
            var sponsor = new Sponsor
            {
                SponsorId = "DEMO-SP005",
                SponsorName = "Pacific Resources Ltd",
                LegalName = "Pacific Resources Limited Philippines",
                Address = "Pacific Star Building, Makati Avenue corner Gil Puyat",
                Tin = "567-890-123-000",
                IsActive = false,
                CreatedOn = DateTime.UtcNow.AddMonths(-12),
                CreatedByUserId = adminUserId,
                ModifiedOn = DateTime.UtcNow.AddMonths(-1)
            };
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _context.Set<SponsorContact>().Add(new SponsorContact
            {
                SponsorId = "DEMO-SP005",
                Name = "Michael Chen",
                Email = "m.chen@pacificres.com",
                Phone = "+63 2 8845 6789",
                IsActive = false
            });

            _context.Set<SponsorAddress>().Add(new SponsorAddress
            {
                SponsorId = "DEMO-SP005",
                AddressType = "Billing",
                AddressLine1 = "Pacific Star Building",
                AddressLine2 = "Makati Avenue corner Gil Puyat",
                City = "Makati",
                StateProvince = "Metro Manila",
                PostalCode = "1200",
                Country = "Philippines",
                IsPrimary = true,
                IsActive = false
            });
        }

        // DEMO-SP006: Global Tech Corp (duplicate for merge testing)
        if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == "DEMO-SP006"))
        {
            var sponsor = new Sponsor
            {
                SponsorId = "DEMO-SP006",
                SponsorName = "Global Tech Corp",
                LegalName = "Global Tech Corporation (duplicate)",
                Address = "BGC Corporate Tower, Taguig",
                Tin = "123-456-789-001",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-1),
                CreatedByUserId = adminUserId
            };
            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _context.Set<SponsorContact>().Add(new SponsorContact
            {
                SponsorId = "DEMO-SP006",
                Name = "Maria Santos",
                Email = "maria@globaltech.com",
                Phone = "+63 2 8123 4500",
                IsActive = true
            });

            _context.Set<SponsorAddress>().Add(new SponsorAddress
            {
                SponsorId = "DEMO-SP006",
                AddressType = "Billing",
                AddressLine1 = "BGC Corporate Tower",
                City = "Taguig",
                StateProvince = "Metro Manila",
                PostalCode = "1634",
                Country = "Philippines",
                IsPrimary = true,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoStudentsAsync()
    {
        var students = new[]
        {
            new Student { StudentId = "DEMO-ST001", FirstName = "Emma", LastName = "Wilson", GradeLevel = "Grade 5", SponsorId = "DEMO-SP001", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST002", FirstName = "Liam", LastName = "Anderson", GradeLevel = "Grade 8", SponsorId = "DEMO-SP001", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST003", FirstName = "Sophia", LastName = "Lee", GradeLevel = "Grade 3", SponsorId = "DEMO-SP002", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST004", FirstName = "Noah", LastName = "Kim", GradeLevel = "Grade 11", SponsorId = "DEMO-SP002", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST005", FirstName = "Olivia", LastName = "Johnson", GradeLevel = "Grade 6", SponsorId = "DEMO-SP003", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST006", FirstName = "Ethan", LastName = "Martinez", GradeLevel = "Grade 9", SponsorId = "DEMO-SP004", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST007", FirstName = "Ava", LastName = "Garcia", GradeLevel = "Grade 4", SponsorId = "DEMO-SP004", SchoolYearId = "25-26", StudentStatus = "Active" },
            new Student { StudentId = "DEMO-ST008", FirstName = "Mason", LastName = "Chen", GradeLevel = "Grade 10", SponsorId = "DEMO-SP005", SchoolYearId = "25-26", StudentStatus = "Withdrawn" }
        };

        foreach (var student in students)
        {
            if (!await _context.Students.AnyAsync(s => s.StudentId == student.StudentId && s.SchoolYearId == student.SchoolYearId))
            {
                _context.Students.Add(student);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoLoGsAsync()
    {
        var adminUser = await _userManager.FindByEmailAsync("demo.admin@ism.edu.ph");
        var adminUserId = adminUser?.Id ?? "system";

        // DEMO-LOG001 for DEMO-SP001
        if (!await _context.LogCoverages.AnyAsync(l => l.SponsorId == "DEMO-SP001" && l.SchoolYearId == "25-26"))
        {
            var log1 = new LogCoverage
            {
                SponsorId = "DEMO-SP001",
                SchoolYearId = "25-26",
                LogStatus = "Approved",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-5),
                CreatedByUserId = adminUserId,
                ActivatedOn = DateTime.UtcNow.AddMonths(-5),
                ActivatedByUserId = adminUserId,
                EffectiveFrom = new DateTime(2025, 8, 1),
                EffectiveTo = new DateTime(2026, 6, 30)
            };
            _context.LogCoverages.Add(log1);
            await _context.SaveChangesAsync();

            var rules1 = new[]
            {
                new LoGCoverageRule { LogId = log1.LogId, CoverageTarget = "Item", ItemId = "TUITION-ES", CoverageType = "Percentage", CoveragePercentage = 100, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-5), DisplayOrder = 1 },
                new LoGCoverageRule { LogId = log1.LogId, CoverageTarget = "Item", ItemId = "TUITION-MS", CoverageType = "Percentage", CoveragePercentage = 100, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-5), DisplayOrder = 2 },
                new LoGCoverageRule { LogId = log1.LogId, CoverageTarget = "Item", ItemId = "BUS-SERVICE", CoverageType = "Percentage", CoveragePercentage = 70, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-5), DisplayOrder = 3 },
                new LoGCoverageRule { LogId = log1.LogId, CoverageTarget = "Item", ItemId = "LUNCH-PLAN", CoverageType = "Percentage", CoveragePercentage = 50, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-5), DisplayOrder = 4 }
            };
            foreach (var rule in rules1)
            {
                _context.Set<LoGCoverageRule>().Add(rule);
            }
        }

        // DEMO-LOG002 for DEMO-SP002
        if (!await _context.LogCoverages.AnyAsync(l => l.SponsorId == "DEMO-SP002" && l.SchoolYearId == "25-26"))
        {
            var log2 = new LogCoverage
            {
                SponsorId = "DEMO-SP002",
                SchoolYearId = "25-26",
                LogStatus = "Approved",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-4),
                CreatedByUserId = adminUserId,
                ActivatedOn = DateTime.UtcNow.AddMonths(-4),
                ActivatedByUserId = adminUserId,
                EffectiveFrom = new DateTime(2025, 8, 1),
                EffectiveTo = new DateTime(2026, 6, 30)
            };
            _context.LogCoverages.Add(log2);
            await _context.SaveChangesAsync();

            var rules2 = new[]
            {
                new LoGCoverageRule { LogId = log2.LogId, CoverageTarget = "Item", ItemId = "TUITION-ES", CoverageType = "Percentage", CoveragePercentage = 100, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-4), DisplayOrder = 1 },
                new LoGCoverageRule { LogId = log2.LogId, CoverageTarget = "Item", ItemId = "TUITION-HS", CoverageType = "Percentage", CoveragePercentage = 100, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-4), DisplayOrder = 2 },
                new LoGCoverageRule { LogId = log2.LogId, CoverageTarget = "Item", ItemId = "TECHNOLOGY", CoverageType = "Percentage", CoveragePercentage = 100, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-4), DisplayOrder = 3 },
                new LoGCoverageRule { LogId = log2.LogId, CoverageTarget = "Item", ItemId = "TEXTBOOK", CoverageType = "Percentage", CoveragePercentage = 80, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-4), DisplayOrder = 4 }
            };
            foreach (var rule in rules2)
            {
                _context.Set<LoGCoverageRule>().Add(rule);
            }
        }

        // DEMO-LOG003 for DEMO-SP003
        if (!await _context.LogCoverages.AnyAsync(l => l.SponsorId == "DEMO-SP003" && l.SchoolYearId == "25-26"))
        {
            var log3 = new LogCoverage
            {
                SponsorId = "DEMO-SP003",
                SchoolYearId = "25-26",
                LogStatus = "Approved",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddMonths(-3),
                CreatedByUserId = adminUserId,
                ActivatedOn = DateTime.UtcNow.AddMonths(-3),
                ActivatedByUserId = adminUserId,
                EffectiveFrom = new DateTime(2025, 8, 1),
                EffectiveTo = new DateTime(2026, 6, 30)
            };
            _context.LogCoverages.Add(log3);
            await _context.SaveChangesAsync();

            var rules3 = new[]
            {
                new LoGCoverageRule { LogId = log3.LogId, CoverageTarget = "Item", ItemId = "TUITION-MS", CoverageType = "Percentage", CoveragePercentage = 100, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-3), DisplayOrder = 1 },
                new LoGCoverageRule { LogId = log3.LogId, CoverageTarget = "Item", ItemId = "BUS-SERVICE", CoverageType = "Percentage", CoveragePercentage = 0, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-3), DisplayOrder = 2 },
                new LoGCoverageRule { LogId = log3.LogId, CoverageTarget = "Item", ItemId = "UNIFORM", CoverageType = "Percentage", CoveragePercentage = 0, IsActive = true, CreatedOn = DateTime.UtcNow.AddMonths(-3), DisplayOrder = 3 }
            };
            foreach (var rule in rules3)
            {
                _context.Set<LoGCoverageRule>().Add(rule);
            }
        }

        // DEMO-LOG004 for DEMO-SP004 (under review)
        if (!await _context.LogCoverages.AnyAsync(l => l.SponsorId == "DEMO-SP004" && l.SchoolYearId == "25-26"))
        {
            var log4 = new LogCoverage
            {
                SponsorId = "DEMO-SP004",
                SchoolYearId = "25-26",
                LogStatus = "UnderReview",
                IsActive = false,
                CreatedOn = DateTime.UtcNow.AddDays(-7),
                CreatedByUserId = adminUserId
            };
            _context.LogCoverages.Add(log4);
        }

        // DEMO-LOG005 for DEMO-SP002 (previous year, inactive)
        if (!await _context.LogCoverages.AnyAsync(l => l.SponsorId == "DEMO-SP002" && l.SchoolYearId == "24-25"))
        {
            var log5 = new LogCoverage
            {
                SponsorId = "DEMO-SP002",
                SchoolYearId = "24-25",
                LogStatus = "Approved",
                IsActive = false,
                CreatedOn = DateTime.UtcNow.AddMonths(-12),
                CreatedByUserId = adminUserId,
                ActivatedOn = DateTime.UtcNow.AddMonths(-12),
                ActivatedByUserId = adminUserId,
                DeactivatedOn = DateTime.UtcNow.AddMonths(-2),
                DeactivatedByUserId = adminUserId,
                DeactivationReason = "School year ended"
            };
            _context.LogCoverages.Add(log5);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoChangeRequestsAsync()
    {
        var admissionsUser = await _userManager.FindByEmailAsync("demo.admissions@ism.edu.ph");
        var adminUser = await _userManager.FindByEmailAsync("demo.admin@ism.edu.ph");
        var requesterId = admissionsUser?.Id ?? "system";
        var resolverId = adminUser?.Id ?? "system";

        var changeRequests = new[]
        {
            new ChangeRequest
            {
                SponsorId = "DEMO-SP001",
                Field = "ContactEmail",
                FieldLabel = "Contact Email",
                CurrentValue = "maria.santos@globaltech.com",
                NewValue = "m.santos@globaltech.ph",
                Status = "pending",
                RequestedOn = DateTime.UtcNow.AddDays(-3),
                RequestedByUserId = requesterId
            },
            new ChangeRequest
            {
                SponsorId = "DEMO-SP002",
                Field = "ContactPhone",
                FieldLabel = "Contact Phone",
                CurrentValue = "+63 2 8632 4444",
                NewValue = "+63 2 8632 5000",
                Status = "approved",
                RequestedOn = DateTime.UtcNow.AddDays(-10),
                RequestedByUserId = requesterId,
                ResolvedOn = DateTime.UtcNow.AddDays(-9),
                ResolvedByUserId = resolverId
            },
            new ChangeRequest
            {
                SponsorId = "DEMO-SP003",
                Field = "Address",
                FieldLabel = "Address",
                CurrentValue = "Tower 2, RCBC Plaza",
                NewValue = "Tower 3, RCBC Plaza",
                Status = "rejected",
                RequestedOn = DateTime.UtcNow.AddDays(-15),
                RequestedByUserId = requesterId,
                ResolvedOn = DateTime.UtcNow.AddDays(-14),
                ResolvedByUserId = resolverId
            },
            new ChangeRequest
            {
                SponsorId = "DEMO-SP004",
                Field = "ContactPerson",
                FieldLabel = "Contact Person",
                CurrentValue = "Ana Reyes",
                NewValue = "Carlos Mendoza",
                Status = "pending",
                RequestedOn = DateTime.UtcNow.AddDays(-1),
                RequestedByUserId = requesterId
            }
        };

        foreach (var request in changeRequests)
        {
            _context.ChangeRequests.Add(request);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoSyncLogsAsync()
    {
        var syncLogs = new[]
        {
            new SyncLog { EntityType = "Sponsor", EntityId = "DEMO-SP001", TargetSystem = "PowerSchool", EventType = "Create", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-30), LastSucceededAt = DateTime.UtcNow.AddDays(-30), RetryCount = 0 },
            new SyncLog { EntityType = "Sponsor", EntityId = "DEMO-SP001", TargetSystem = "NetSuite", EventType = "Create", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-29), LastSucceededAt = DateTime.UtcNow.AddDays(-29), RetryCount = 0 },
            new SyncLog { EntityType = "Sponsor", EntityId = "DEMO-SP002", TargetSystem = "PowerSchool", EventType = "Create", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-25), LastSucceededAt = DateTime.UtcNow.AddDays(-25), RetryCount = 0 },
            new SyncLog { EntityType = "Sponsor", EntityId = "DEMO-SP003", TargetSystem = "NetSuite", EventType = "Create", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-20), LastSucceededAt = DateTime.UtcNow.AddDays(-20), RetryCount = 0 },
            new SyncLog { EntityType = "Sponsor", EntityId = "DEMO-SP004", TargetSystem = "PowerSchool", EventType = "Create", Status = "Failed", AttemptedAt = DateTime.UtcNow.AddDays(-15), RetryCount = 2, ErrorMessage = "Connection timeout to PowerSchool API", ResponsePayload = "Connection timeout to PowerSchool API" },
            new SyncLog { EntityType = "LogCoverage", EntityId = "1", TargetSystem = "NetSuite", EventType = "Create", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-28), LastSucceededAt = DateTime.UtcNow.AddDays(-28), RetryCount = 0 },
            new SyncLog { EntityType = "LogCoverage", EntityId = "2", TargetSystem = "NetSuite", EventType = "Create", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-24), LastSucceededAt = DateTime.UtcNow.AddDays(-24), RetryCount = 0 },
            new SyncLog { EntityType = "LogCoverage", EntityId = "3", TargetSystem = "NetSuite", EventType = "Create", Status = "Failed", AttemptedAt = DateTime.UtcNow.AddDays(-19), RetryCount = 1, ErrorMessage = "NetSuite validation error: Invalid sponsor reference", ResponsePayload = "NetSuite validation error: Invalid sponsor reference" },
            new SyncLog { EntityType = "Student", EntityId = "DEMO-ST001", TargetSystem = "PowerSchool", EventType = "Update", Status = "Succeeded", AttemptedAt = DateTime.UtcNow.AddDays(-10), LastSucceededAt = DateTime.UtcNow.AddDays(-10), RetryCount = 0 }
        };

        foreach (var log in syncLogs)
        {
            _context.Set<SyncLog>().Add(log);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoAuditLogsAsync()
    {
        var adminUser = await _userManager.FindByEmailAsync("demo.admin@ism.edu.ph");
        var admissionsUser = await _userManager.FindByEmailAsync("demo.admissions@ism.edu.ph");
        var adminDisplay = adminUser?.DisplayName ?? "System";
        var admissionsDisplay = admissionsUser?.DisplayName ?? "System";

        var auditLogs = new[]
        {
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-30), Item = "Sponsor", Details = "Created sponsor DEMO-SP001", UserDisplay = adminDisplay, RoleName = "Admin", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-29), Item = "Sponsor", Details = "Created sponsor DEMO-SP002", UserDisplay = adminDisplay, RoleName = "Admin", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-28), Item = "LogCoverage", Details = "Created LoG for sponsor DEMO-SP001", UserDisplay = admissionsDisplay, RoleName = "Admissions", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-28), Item = "LogCoverage", Details = "Activated LoG for sponsor DEMO-SP001", UserDisplay = adminDisplay, RoleName = "Admin", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-25), Item = "LogCoverage", Details = "Created LoG for sponsor DEMO-SP002", UserDisplay = admissionsDisplay, RoleName = "Admissions", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-24), Item = "LogCoverage", Details = "Activated LoG for sponsor DEMO-SP002", UserDisplay = adminDisplay, RoleName = "Admin", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-10), Item = "ChangeRequest", Details = "Submitted change request for DEMO-SP002 contact phone", UserDisplay = admissionsDisplay, RoleName = "Admissions", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-9), Item = "ChangeRequest", Details = "Approved change request for DEMO-SP002", UserDisplay = adminDisplay, RoleName = "Admin", SchoolYearId = "25-26" },
            new ActivityLog { Date = DateTime.UtcNow.AddDays(-3), Item = "ChangeRequest", Details = "Submitted change request for DEMO-SP001 email", UserDisplay = admissionsDisplay, RoleName = "Admissions", SchoolYearId = "25-26" }
        };

        foreach (var log in auditLogs)
        {
            _context.ActivityLogs.Add(log);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoCoverageEvaluationsAsync()
    {
        var adminUser = await _userManager.FindByEmailAsync("demo.admin@ism.edu.ph");
        var adminUserId = adminUser?.Id ?? "system";
        var adminDisplay = adminUser?.DisplayName ?? "System";

        var evaluations = new[]
        {
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-20),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP001",
                StudentId = "DEMO-ST001",
                LogId = 1,
                ItemId = "TUITION-ES",
                RequestedAmount = 450000m,
                ChargeDate = DateTime.UtcNow.AddDays(-20),
                SponsorAmount = 450000m,
                ParentAmount = 0m,
                Decision = "Covered",
                BillTo = "Sponsor",
                ReasonCode = "FULL_COVERAGE",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-20),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP001",
                StudentId = "DEMO-ST002",
                LogId = 1,
                ItemId = "TUITION-MS",
                RequestedAmount = 520000m,
                ChargeDate = DateTime.UtcNow.AddDays(-20),
                SponsorAmount = 520000m,
                ParentAmount = 0m,
                Decision = "Covered",
                BillTo = "Sponsor",
                ReasonCode = "FULL_COVERAGE",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-18),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP001",
                StudentId = "DEMO-ST001",
                LogId = 1,
                ItemId = "BUS-SERVICE",
                RequestedAmount = 80000m,
                ChargeDate = DateTime.UtcNow.AddDays(-18),
                SponsorAmount = 56000m,
                ParentAmount = 24000m,
                Decision = "Split",
                BillTo = "Split",
                ReasonCode = "PARTIAL_COVERAGE",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-15),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP002",
                StudentId = "DEMO-ST003",
                LogId = 2,
                ItemId = "TUITION-ES",
                RequestedAmount = 450000m,
                ChargeDate = DateTime.UtcNow.AddDays(-15),
                SponsorAmount = 450000m,
                ParentAmount = 0m,
                Decision = "Covered",
                BillTo = "Sponsor",
                ReasonCode = "FULL_COVERAGE",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-15),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP002",
                StudentId = "DEMO-ST004",
                LogId = 2,
                ItemId = "TUITION-HS",
                RequestedAmount = 580000m,
                ChargeDate = DateTime.UtcNow.AddDays(-15),
                SponsorAmount = 580000m,
                ParentAmount = 0m,
                Decision = "Covered",
                BillTo = "Sponsor",
                ReasonCode = "FULL_COVERAGE",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-12),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP003",
                StudentId = "DEMO-ST005",
                LogId = 3,
                ItemId = "TUITION-MS",
                RequestedAmount = 520000m,
                ChargeDate = DateTime.UtcNow.AddDays(-12),
                SponsorAmount = 520000m,
                ParentAmount = 0m,
                Decision = "Covered",
                BillTo = "Sponsor",
                ReasonCode = "FULL_COVERAGE",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-12),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP003",
                StudentId = "DEMO-ST005",
                ItemId = "BUS-SERVICE",
                RequestedAmount = 80000m,
                ChargeDate = DateTime.UtcNow.AddDays(-12),
                SponsorAmount = 0m,
                ParentAmount = 80000m,
                Decision = "NotCovered",
                BillTo = "Parent",
                ReasonCode = "NOT_COVERED",
                Success = true
            },
            new CoverageEvaluationAudit
            {
                EvaluatedOn = DateTime.UtcNow.AddDays(-7),
                EvaluatedByUserId = adminUserId,
                EvaluatedByUserDisplay = adminDisplay,
                EvaluatedByRole = "Admin",
                SchoolYearId = "25-26",
                SponsorId = "DEMO-SP004",
                StudentId = "DEMO-ST006",
                ItemId = "TUITION-MS",
                RequestedAmount = 520000m,
                ChargeDate = DateTime.UtcNow.AddDays(-7),
                SponsorAmount = 0m,
                ParentAmount = 0m,
                Decision = "NotCovered",
                BillTo = "Parent",
                ReasonCode = "NO_ACTIVE_LOG",
                Success = false,
                ErrorMessage = "No active LoG found for sponsor"
            }
        };

        foreach (var eval in evaluations)
        {
            _context.Set<CoverageEvaluationAudit>().Add(eval);
        }

        await _context.SaveChangesAsync();
    }
}
