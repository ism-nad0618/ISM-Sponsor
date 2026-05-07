using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ISMSponsor.Models.API;
using ISMSponsor.Data;

namespace ISMSponsor.Tests.Api;

public class CoverageTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private bool _seeded = false;
    private static int _instanceCounter = 0;
    private readonly string _dbName;
    
    public CoverageTestWebApplicationFactory()
    {
        // Use a unique database name for each test factory instance
        _dbName = $"ISMSponsorTestDB_{Interlocked.Increment(ref _instanceCounter)}_{Guid.NewGuid():N}";
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(ResolveContentRoot());
        
        // Override database connection to use unique in-memory database
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            // Add new DbContext with unique database name
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        
        // Seed database after host is created
        if (!_seeded)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var db = services.GetRequiredService<AppDbContext>();
                
                // Database is fresh with unique name, just initialize
                Console.WriteLine($"Initializing test database: {_dbName}");
                
                var initializer = services.GetRequiredService<DbInitializer>();
                initializer.Initialize();
                
                var seeder = services.GetRequiredService<DemoDataSeeder>();
                seeder.SeedDemoDataAsync().GetAwaiter().GetResult();
                
                _seeded = true;
                Console.WriteLine("Test database seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding test data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        return host;
    }

    private static string ResolveContentRoot()
    {
        var current = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(current);

        while (directory != null)
        {
            var csprojPath = Path.Combine(directory.FullName, "ISMSponsor.csproj");
            if (File.Exists(csprojPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return current;
    }
}

/// <summary>
/// Integration tests for Coverage Evaluation API
/// Tests the POST /api/v1/coverage/preview endpoint with real scenarios
/// </summary>
public class CoverageEvaluationTests : IClassFixture<CoverageTestWebApplicationFactory>
{
    private readonly CoverageTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

     public CoverageEvaluationTests(CoverageTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Tests verifying that SP002 sponsor exists in test database
    /// </summary>
    [Fact]
    public async Task Database_HasSP002Sponsor()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/sponsors/SP002");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var sponsor = await response.Content.ReadAsStringAsync();
        Assert.Contains("SP002", sponsor);
        Assert.Contains("Ayala Holdings", sponsor);
    }

    /// <summary>
    /// Tests verifying that STUD006 student has an active LoG with $1000 cap
    /// </summary>
    [Fact]
    public async Task Database_STUD006_HasActiveLoGWithCap()
    {
        // Arrange: Test with $100 charge that should be fully covered
        var request = new CoverageEvaluationRequest
        {
            StudentId = "STUD006",
            SchoolYearId = "25-26",
            ItemId = "MAJOR-SLSP-G06 SLSP FULL",
            ChargeDescription = "Test",
            Amount = 100.00m,
            Currency = "US Dollar",
            ChargeDate = new DateTime(2026, 5, 6)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/coverage/preview", request);
        var responseText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Preview $100 response: {responseText}");
        
        // Assert: Should be fully covered, NOT NO_ACTIVE_LOG
        var result = await response.Content.ReadFromJsonAsync<CoverageEvaluationResponse>();
        Assert.NotNull(result);
        Assert.NotEqual("NO_ACTIVE_LOG", result.ReasonCode);
        Assert.Equal(CoverageDecision.Covered, result.Decision);
    }

    /// <summary>
    /// Tests the STUD006/Ayala Holdings scenario with EXCEEDS_CAP billing split
    /// Verifies that a $1500 charge with $1000 cap results in Split decision with two allocations
    /// Tests both /evaluate and /preview endpoints
    /// </summary>
    [Fact]
    public async Task Evaluate_STUD006_AyalaHoldings_ExceedsCap_ReturnsSplitWithAllocations()
    {
        // Arrange: Sample request from user requirements
        var request = new CoverageEvaluationRequest
        {
            StudentId = "STUD006",
            SchoolYearId = "25-26",
            ItemId = "MAJOR-SLSP-G06 SLSP FULL",
            ChargeDescription = "Specialized Learning Support Program Fee (Full Semester Fees)",
            Amount = 1500.00m,
            Currency = "US Dollar",
            ChargeDate = new DateTime(2026, 5, 6)
        };

        // Act: Call evaluate endpoint
        var response = await _client.PostAsJsonAsync("/api/v1/coverage/evaluate", request);

        // Assert: Verify response status
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CoverageEvaluationResponse>();
        
        // Assert: Verify decision and amounts
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(CoverageDecision.Split, result.Decision);
        Assert.Equal(BillTo.SponsorAndParent, result.BillTo);
        
        // Assert: Verify student information
        Assert.Equal("STUD006", result.StudentId);
        Assert.Equal("Renee Tan", result.StudentName);
        
        // Assert: Verify sponsor information
        Assert.Equal("SP002", result.SponsorId);
        Assert.Equal("Ayala Holdings", result.SponsorName);
        
        // Assert: Verify amounts
        Assert.Equal(1000.00m, result.SponsorAmount);
        Assert.Equal(500.00m, result.ParentAmount);
        Assert.Equal(1500.00m, result.TotalAmount);
        
        // Assert: Verify reason code
        Assert.Equal("EXCEEDS_CAP", result.ReasonCode);
        Assert.Contains("exceeds", result.Explanation, StringComparison.OrdinalIgnoreCase);
        
        // Assert: Verify allocations array
        Assert.NotNull(result.Allocations);
        Assert.Equal(2, result.Allocations.Count);
        
        // Assert: Verify Sponsor allocation
        var sponsorAllocation = result.Allocations[0];
        Assert.Equal("Sponsor", sponsorAllocation.PartyType);
        Assert.Equal("SP002", sponsorAllocation.PartyId);
        Assert.Equal("Ayala Holdings", sponsorAllocation.PartyName);
        Assert.Equal(1000.00m, sponsorAllocation.Amount);
        Assert.Equal("US Dollar", sponsorAllocation.Currency);
        Assert.Equal("Sponsor", sponsorAllocation.BillTo);
        Assert.Equal("MAJOR-SLSP-G06 SLSP FULL", sponsorAllocation.ChargeCode);
        
        // Assert: Verify Parent allocation
        var parentAllocation = result.Allocations[1];
        Assert.Equal("Parent", parentAllocation.PartyType);
        Assert.Null(parentAllocation.PartyId);
        Assert.Null(parentAllocation.PartyName);
        Assert.Equal(500.00m, parentAllocation.Amount);
        Assert.Equal("US Dollar", parentAllocation.Currency);
        Assert.Equal("Parent", parentAllocation.BillTo);
        Assert.Equal("MAJOR-SLSP-G06 SLSP FULL", parentAllocation.ChargeCode);
        
        // Assert: Verify percentages
        Assert.InRange(result.SponsorPercent, 66.5m, 67.0m); // ~66.67%
        Assert.InRange(result.ParentPercent, 33.0m, 33.5m);  // ~33.33%
        
        // Assert: Verify audit fields
        Assert.NotNull(result.CorrelationId);
        Assert.NotNull(result.DecisionId);
        Assert.NotNull(result.RuleSnapshot);
        Assert.Contains("SP002", result.RuleSnapshot);
        Assert.Contains("Ayala Holdings", result.RuleSnapshot);
        Assert.Contains("1,000", result.RuleSnapshot); // Cap amount is formatted with comma
    }

    /// <summary>
    /// Tests the STUD006/Ayala Holdings scenario with EXCEEDS_CAP billing split on preview endpoint
    /// </summary>
    [Fact]
    public async Task Preview_STUD006_AyalaHoldings_ExceedsCap_ReturnsSplitWithAllocations()
    {
        // Arrange: Sample request
        var request = new CoverageEvaluationRequest
        {
            StudentId = "STUD006",
            SchoolYearId = "25-26",
            ItemId = "MAJOR-SLSP-G06 SLSP FULL",
            ChargeDescription = "Specialized Learning Support Program Fee (Full Semester Fees)",
            Amount = 1500.00m,
            Currency = "US Dollar",
            ChargeDate = new DateTime(2026, 5, 6)
        };

        // Act: Call preview endpoint
        var response = await _client.PostAsJsonAsync("/api/v1/coverage/preview", request);

        // Debug output
        var responseText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Preview $1500 response: {responseText}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<CoverageEvaluationResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(CoverageDecision.Split, result.Decision);
        Assert.Equal(BillTo.SponsorAndParent, result.BillTo);
        Assert.Equal("STUD006", result.StudentId);
        Assert.Equal("Renee Tan", result.StudentName);
        Assert.Equal("SP002", result.SponsorId);
        Assert.Equal(1000.00m, result.SponsorAmount);
        Assert.Equal(500.00m, result.ParentAmount);
        Assert.Equal(2, result.Allocations?.Count);
    }

    /// <summary>
    /// Tests fully covered scenario (charge below cap)
    /// </summary>
    [Fact]
    public async Task Evaluate_STUD006_FullyCovered_ReturnsCoveredWithSingleAllocation()
    {
        // Arrange: Charge below the $1000 cap
        var request = new CoverageEvaluationRequest
        {
            StudentId = "STUD006",
            SchoolYearId = "25-26",
            ItemId = "MAJOR-SLSP-G06 SLSP FULL",
            ChargeDescription = "Specialized Learning Support Program Fee (Full Semester Fees)",
            Amount = 800.00m,
            Currency = "US Dollar",
            ChargeDate = new DateTime(2026, 5, 6)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/coverage/evaluate", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<CoverageEvaluationResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(CoverageDecision.Covered, result.Decision);
        Assert.Equal(BillTo.Sponsor, result.BillTo);
        Assert.Equal("STUD006", result.StudentId);
        Assert.Equal("Renee Tan", result.StudentName);
        Assert.Equal("SP002", result.SponsorId);
        Assert.Equal("Ayala Holdings", result.SponsorName);
        Assert.Equal(800.00m, result.SponsorAmount);
        Assert.Equal(0.00m, result.ParentAmount);
        Assert.Equal(800.00m, result.TotalAmount);
        Assert.Equal("COVERED", result.ReasonCode);
        
        // Verify single sponsor allocation
        Assert.NotNull(result.Allocations);
        Assert.Single(result.Allocations);
        var allocation = result.Allocations[0];
        Assert.Equal("Sponsor", allocation.PartyType);
        Assert.Equal("SP002", allocation.PartyId);
        Assert.Equal("Ayala Holdings", allocation.PartyName);
        Assert.Equal(800.00m, allocation.Amount);
    }
    [Fact]
    public async Task Preview_NoActiveLog_ReturnsNotCovered()
    {
        // Arrange: Request for student with no LoG
        var request = new CoverageEvaluationRequest
        {
            StudentId = "NONEXISTENT",
            SchoolYearId = "25-26",
            ItemId = "TUITION-ES",
            Amount = 1000.00m,
            ChargeDate = new DateTime(2026, 5, 6)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/coverage/preview", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<CoverageEvaluationResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(CoverageDecision.NotCovered, result.Decision);
        Assert.Equal(BillTo.Parent, result.BillTo);
        Assert.Equal("NO_ACTIVE_LOG", result.ReasonCode);
        Assert.Equal(1000.00m, result.ParentAmount);
        Assert.Equal(0m, result.SponsorAmount);
        
        // Verify single parent allocation
        Assert.NotNull(result.Allocations);
        Assert.Single(result.Allocations);
        var allocation = result.Allocations[0];
        Assert.Equal("Parent", allocation.PartyType);
        Assert.Null(allocation.PartyId);
        Assert.Null(allocation.PartyName);
        Assert.Equal(1000.00m, allocation.Amount);
    }

    /// <summary>
    /// Tests GET /api/v1/sponsors/SP002 returns Ayala Holdings
    /// </summary>
    [Fact]
    public async Task GetSponsor_SP002_ReturnsAyalaHoldings()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/sponsors/SP002");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("SP002", json);
        Assert.Contains("Ayala Holdings", json);
    }
}
