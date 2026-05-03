using ISMSponsor.Data;
using ISMSponsor.Models;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Data;

/// <summary>
/// Medium-sized comprehensive demo data seeder for UAT testing
/// Creates: 10 sponsors, 100 students, 100 LoGs, 20 users (5 per role)
/// </summary>
public class MediumDemoDataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public MediumDemoDataSeeder(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedMediumDemoDataAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("Starting MEDIUM demo data seed...");
        Console.WriteLine("========================================");

        // Check if demo data already exists
        var existingSponsors = await _context.Sponsors.CountAsync(s => s.SponsorId.StartsWith("DEMO-"));
        if (existingSponsors >= 10)
        {
            Console.WriteLine($"⚠️  Demo data already exists ({existingSponsors} sponsors). Skipping seed.");
            return;
        }

        try
        {
            // 1. School years
            await SeedSchoolYearsAsync();

            // 2. Items and categories
            await SeedItemsAndCategoriesAsync();

            // 3. Demo users (5 per role = 20 total)
            await SeedDemoUsersAsync();

            // 4. Demo sponsors (10 total)
            await SeedDemoSponsorsAsync();

            // 5. Demo students (100 total, 10 per sponsor)
            await SeedDemoStudentsAsync();

            // 6. Demo LoGs with coverage rules
            await SeedDemoLoGsAsync();

            await _context.SaveChangesAsync();

            // Print summary
            await PrintSummaryAsync();

            Console.WriteLine("========================================");
            Console.WriteLine("✅ Medium demo data seed completed!");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during demo data seed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task SeedSchoolYearsAsync()
    {
        Console.WriteLine("\n📅 Setting up school years...");

        var schoolYears = new[]
        {
            new SchoolYear
            {
                SchoolYearId = "2024-2025",
                Name = "2024-2025",
                ValidFrom = new DateTime(2024, 8, 15),
                ValidTo = new DateTime(2025, 5, 30),
                IsActive = false
            },
            new SchoolYear
            {
                SchoolYearId = "2025-2026",
                Name = "2025-2026",
                ValidFrom = new DateTime(2025, 8, 15),
                ValidTo = new DateTime(2026, 5, 30),
                IsActive = true
            },
            new SchoolYear
            {
                SchoolYearId = "2026-2027",
                Name = "2026-2027",
                ValidFrom = new DateTime(2026, 8, 15),
                ValidTo = new DateTime(2027, 5, 30),
                IsActive = false
            }
        };

        foreach (var sy in schoolYears)
        {
            if (!await _context.SchoolYears.AnyAsync(s => s.SchoolYearId == sy.SchoolYearId))
            {
                _context.SchoolYears.Add(sy);
                Console.WriteLine($"  ✓ Created school year: {sy.Name}");
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedItemsAndCategoriesAsync()
    {
        Console.WriteLine("\n📦 Setting up items and categories...");

        // Categories
        var categories = new[]
        {
            new ItemCategory { CategoryId = "TUITION", CategoryName = "Tuition Fees", Description = "Academic tuition and registration fees", IsActive = true },
            new ItemCategory { CategoryId = "SUPPLIES", CategoryName = "School Supplies", Description = "Books, materials, and supplies", IsActive = true },
            new ItemCategory { CategoryId = "UNIFORM", CategoryName = "Uniforms", Description = "School uniforms and PE attire", IsActive = true },
            new ItemCategory { CategoryId = "ACTIVITIES", CategoryName = "Activities", Description = "Field trips, sports, clubs", IsActive = true },
            new ItemCategory { CategoryId = "OTHER", CategoryName = "Other Fees", Description = "Miscellaneous school fees", IsActive = true }
        };

        foreach (var category in categories)
        {
            if (!await _context.Set<ItemCategory>().AnyAsync(c => c.CategoryId == category.CategoryId))
            {
                _context.Set<ItemCategory>().Add(category);
            }
        }
        await _context.SaveChangesAsync();

        // Items
        var items = new[]
        {
            new Item { ItemId = "TUITION-ELEM", ItemName = "Elementary Tuition", GradeLevel = "ES", CategoryId = "TUITION", IsActive = true },
            new Item { ItemId = "TUITION-HS", ItemName = "High School Tuition", GradeLevel = "HS", CategoryId = "TUITION", IsActive = true },
            new Item { ItemId = "BOOKS", ItemName = "Textbooks", GradeLevel = "ALL", CategoryId = "SUPPLIES", IsActive = true },
            new Item { ItemId = "UNIFORM", ItemName = "School Uniform", GradeLevel = "ALL", CategoryId = "UNIFORM", IsActive = true },
            new Item { ItemId = "PE-UNIFORM", ItemName = "PE Uniform", GradeLevel = "ALL", CategoryId = "UNIFORM", IsActive = true },
            new Item { ItemId = "SUPPLIES", ItemName = "School Supplies", GradeLevel = "ALL", CategoryId = "SUPPLIES", IsActive = true },
            new Item { ItemId = "MEALS", ItemName = "Meal Plan", GradeLevel = "ALL", CategoryId = "OTHER", IsActive = true },
            new Item { ItemId = "TRANSPORT", ItemName = "Transportation", GradeLevel = "ALL", CategoryId = "OTHER", IsActive = true }
        };

        var itemsCreated = 0;
        foreach (var item in items)
        {
            if (!await _context.Items.AnyAsync(i => i.ItemId == item.ItemId))
            {
                _context.Items.Add(item);
                itemsCreated++;
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Created {itemsCreated} items");
    }

    private async Task SeedDemoUsersAsync()
    {
        Console.WriteLine("\n👥 Creating 20 demo users (5 per role)...");

        var roles = new[] { "Admin", "Admissions", "Cashier", "Sponsor" };
        var firstNames = new[] { "Alice", "Bob", "Claire", "David", "Emma" };
        var usersCreated = 0;

        foreach (var role in roles)
        {
            for (int i = 1; i <= 5; i++)
            {
                var email = $"demo.{role.ToLower()}{i}@ismanila.org";
                var existingUser = await _userManager.FindByEmailAsync(email);

                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        DisplayName = $"{firstNames[i - 1]} {role}",
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user, "DemoPass123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        usersCreated++;
                    }
                    else
                    {
                        Console.WriteLine($"  ⚠️  Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        Console.WriteLine($"  ✓ Created {usersCreated} demo users");
        Console.WriteLine("  ℹ️  Default password: DemoPass123!");
    }

    private async Task SeedDemoSponsorsAsync()
    {
        Console.WriteLine("\n🏢 Creating 10 demo sponsors...");

        var adminUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email != null && u.Email.Contains("admin"));
        var adminUserId = adminUser?.Id ?? "system";

        var sponsorData = new[]
        {
            new { Name = "Tech Innovations Corp", Contact = "Maria Santos", Email = "maria.santos@techinnovations.ph" },
            new { Name = "Global Finance Bank", Contact = "Robert Chen", Email = "robert.chen@globalfinance.com" },
            new { Name = "PhilHealth Foundation", Contact = "Ana Rodriguez", Email = "ana.rodriguez@philhealth.org" },
            new { Name = "Manila Trading Company", Contact = "Jose Reyes", Email = "jose.reyes@manilatrading.ph" },
            new { Name = "Scholarship Trust Fund", Contact = "Isabel Cruz", Email = "isabel.cruz@scholarshiptrust.org" },
            new { Name = "Community Outreach Org", Contact = "Carlos Garcia", Email = "carlos.garcia@communityoutreach.ph" },
            new { Name = "Education Partners Inc", Contact = "Linda Tan", Email = "linda.tan@educationpartners.com" },
            new { Name = "Youth Development Fund", Contact = "Michael Fernandez", Email = "michael.f@youthdevelopment.org" },
            new { Name = "Corporate Social Resp", Contact = "Patricia Lim", Email = "patricia.lim@csrfund.com" },
            new { Name = "Alumni Association", Contact = "David Santiago", Email = "david.santiago@ismalumni.org" }
        };

        for (int i = 0; i < sponsorData.Length; i++)
        {
            var sponsorId = $"DEMO-SP{(i + 1):D3}";
            var data = sponsorData[i];

            if (!await _context.Sponsors.AnyAsync(s => s.SponsorId == sponsorId))
            {
                var sponsor = new Sponsor
                {
                    SponsorId = sponsorId,
                    SponsorName = data.Name,
                    LegalName = data.Name,
                    Address = $"{i + 1} Sponsor Avenue, BGC, Taguig City",
                    Tin = $"{100 + i}-456-789-000",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow.AddMonths(-(i + 1)),
                    CreatedByUserId = adminUserId,
                    PowerSchoolId = $"PS{i + 1:D3}",
                    NetSuiteId = $"NS{i + 1:D3}"
                };

                _context.Sponsors.Add(sponsor);
                await _context.SaveChangesAsync();

                _context.Set<SponsorContact>().Add(new SponsorContact
                {
                    SponsorId = sponsorId,
                    Name = data.Contact,
                    Email = data.Email,
                    Phone = $"+63-2-{800 + i}-{1000 + i:D4}",
                    IsActive = true
                });

                _context.Set<SponsorAddress>().Add(new SponsorAddress
                {
                    SponsorId = sponsorId,
                    AddressType = "Billing",
                    AddressLine1 = $"{i + 1} Sponsor Avenue",
                    AddressLine2 = "Bonifacio Global City",
                    City = "Taguig",
                    StateProvince = "Metro Manila",
                    PostalCode = "1634",
                    Country = "Philippines",
                    IsPrimary = true,
                    IsActive = true
                });

                Console.WriteLine($"  ✓ Created {sponsorId} - {data.Name}");
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoStudentsAsync()
    {
        Console.WriteLine("\n🎓 Creating 100 demo students (10 per sponsor)...");

        var firstNames = new[] { "Juan", "Maria", "Jose", "Ana", "Pedro", "Isabel", "Carlos", "Rosa", "Miguel", "Sofia",
                                 "Luis", "Carmen", "Antonio", "Teresa", "Francisco", "Patricia", "Manuel", "Elena", "Ramon", "Lucia" };
        var lastNames = new[] { "Santos", "Reyes", "Cruz", "Garcia", "Rodriguez", "Fernandez", "Lopez", "Martinez", "Gonzales", "Perez" };

        var studentsCreated = 0;
        for (int i = 1; i <= 100; i++)
        {
            var studentId = $"DEMO-ST{i:D3}";
            var sponsorId = $"DEMO-SP{((i - 1) / 10) + 1:D3}";

            if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            {
                var firstName = firstNames[(i - 1) % firstNames.Length];
                var lastName = lastNames[(i - 1) % lastNames.Length];
                var gradeLevel = ((i - 1) % 12) + 1;

                var student = new Student
                {
                    StudentId = studentId,
                    FirstName = firstName,
                    LastName = lastName,
                    SchoolYearId = "25-26",
                    GradeLevel = $"STUD-{gradeLevel:D2}",  // Format: STUD-01, STUD-02, etc.
                    SponsorId = sponsorId,
                    StudentStatus = "Active"
                };

                _context.Students.Add(student);
                studentsCreated++;

                if (studentsCreated % 20 == 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"  ✓ Created {studentsCreated} students...");
                }
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Total students created: {studentsCreated}");
    }

    private async Task SeedDemoLoGsAsync()
    {
        Console.WriteLine("\n📄 Creating Letters of Guarantee with coverage rules...");

        var students = await _context.Students
            .Where(s => s.StudentId.StartsWith("DEMO-ST"))
            .ToListAsync();

        var logsCreated = 0;
        var rulesCreated = 0;

        foreach (var student in students)
        {
            var existingLog = await _context.LogCoverages
                .AnyAsync(l => l.StudentId == student.StudentId && l.SchoolYearId == "25-26");

            if (!existingLog)
            {
                var studentNum = int.Parse(student.StudentId.Substring(7));

                var log = new LogCoverage
                {
                    SchoolYearId = "25-26",
                    StudentId = student.StudentId,
                    SponsorId = student.SponsorId!,
                    LogStatus = studentNum % 10 == 0 ? "Submitted" : (studentNum % 15 == 0 ? "Draft" : "Approved"),
                    IsActive = studentNum % 15 != 0,
                    EffectiveFrom = new DateTime(2025, 8, 15),
                    EffectiveTo = new DateTime(2026, 5, 30),
                    Notes = $"Demo LoG for UAT testing - Student {student.StudentId}",
                    CreatedOn = DateTime.UtcNow.AddDays(-30),
                    ModifiedOn = DateTime.UtcNow.AddDays(-30)
                };

                _context.LogCoverages.Add(log);
                await _context.SaveChangesAsync();
                logsCreated++;

                // Add coverage rules
                // Rule 1: Tuition (full or partial)
                var tuitionRule = new LoGCoverageRule
                {
                    LogId = log.LogId,
                    CoverageTarget = "Item",
                    ItemId = studentNum % 3 == 0 ? "TUITION-HS" : "TUITION-ELEM",
                    CoverageType = studentNum % 4 == 0 ? "FixedAmount" : "Full",
                    CoverageFixedAmount = studentNum % 4 == 0 ? 25000m : null,
                    CoveragePercentage = studentNum % 4 == 0 ? null : 100m,
                    CapAmount = studentNum % 3 == 0 ? 55000m : 45000m,
                    ExceptionNote = "Academic fees for SY 2025-2026",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };
                _context.Set<LoGCoverageRule>().Add(tuitionRule);
                rulesCreated++;

                // Rule 2: Books (if tuition is full coverage)
                if (studentNum % 4 != 0)
                {
                    var booksRule = new LoGCoverageRule
                    {
                        LogId = log.LogId,
                        CoverageTarget = "Item",
                        ItemId = "BOOKS",
                        CoverageType = "Percentage",
                        CoveragePercentage = 100m,
                        CapAmount = 3500m,
                        ExceptionNote = "Required textbooks",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Set<LoGCoverageRule>().Add(booksRule);
                    rulesCreated++;
                }

                // Rule 3: Uniform (for some students)
                if (studentNum % 3 == 0)
                {
                    var uniformRule = new LoGCoverageRule
                    {
                        LogId = log.LogId,
                        CoverageTarget = "Item",
                        ItemId = "UNIFORM",
                        CoverageType = "FixedAmount",
                        CoverageFixedAmount = 2500m,
                        CapAmount = 2500m,
                        ExceptionNote = "Standard school uniform",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Set<LoGCoverageRule>().Add(uniformRule);
                    rulesCreated++;
                }

                if (logsCreated % 20 == 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"  ✓ Created {logsCreated} LoGs with rules...");
                }
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"  ✓ Total LoGs created: {logsCreated}");
        Console.WriteLine($"  ✓ Total coverage rules created: {rulesCreated}");
    }

    private async Task PrintSummaryAsync()
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("📊 Demo Data Summary");
        Console.WriteLine("========================================");

        var sponsorCount = await _context.Sponsors.CountAsync(s => s.SponsorId.StartsWith("DEMO-"));
        var studentCount = await _context.Students.CountAsync(s => s.StudentId.StartsWith("DEMO-"));
        var logCount = await _context.LogCoverages.CountAsync(l => l.SponsorId.StartsWith("DEMO-"));
        var ruleCount = await _context.Set<LoGCoverageRule>()
            .CountAsync(r => _context.LogCoverages.Any(l => l.LogId == r.LogId && l.SponsorId.StartsWith("DEMO-")));
        var userCount = await _userManager.Users.CountAsync(u => u.Email!.StartsWith("demo."));
        var itemCount = await _context.Items.CountAsync(i => i.IsActive);

        Console.WriteLine($"  Sponsors: {sponsorCount}");
        Console.WriteLine($"  Students: {studentCount}");
        Console.WriteLine($"  Letters of Guarantee: {logCount}");
        Console.WriteLine($"  Coverage Rules: {ruleCount}");
        Console.WriteLine($"  Demo Users: {userCount}");
        Console.WriteLine($"  Active Items: {itemCount}");
        Console.WriteLine("\n  Demo User Credentials:");
        Console.WriteLine("    Email: demo.admin1 through demo.admin5@ismanila.org");
        Console.WriteLine("    Email: demo.admissions1 through demo.admissions5@ismanila.org");
        Console.WriteLine("    Email: demo.cashier1 through demo.cashier5@ismanila.org");
        Console.WriteLine("    Email: demo.sponsor1 through demo.sponsor5@ismanila.org");
        Console.WriteLine("    Password: DemoPass123!");
        Console.WriteLine("========================================");
    }
}
