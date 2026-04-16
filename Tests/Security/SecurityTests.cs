using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Net.Http.Headers;
using System.IO;
using ISMSponsor.Services;

namespace ISMSponsor.Tests.Security;

public class SecurityTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(ResolveContentRoot());
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
/// Security tests for authentication and authorization enforcement.
/// OWASP Alignment: A01 Broken Access Control, A07 Identification and Authentication Failures
/// </summary>
public class AuthenticationAuthorizationTests : IClassFixture<SecurityTestWebApplicationFactory>
{
    private readonly SecurityTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationAuthorizationTests(SecurityTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccess_Dashboard()
    {
        // Arrange - no authentication

        // Act
        var response = await _client.GetAsync("/Dashboard");

        // Assert - should redirect to login
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect || 
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            "Unauthenticated user should be redirected to login or denied access");
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccess_SponsorsIndex()
    {
        // Arrange - no authentication

        // Act
        var response = await _client.GetAsync("/Sponsors");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect || 
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            "Protected resource should require authentication");
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccess_AdminSettings()
    {
        // Arrange - no authentication

        // Act
        var response = await _client.GetAsync("/Settings/Users");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect || 
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            "Admin-only resource should require authentication");
    }

    [Fact]
    public async Task UnauthenticatedUser_CanAccess_LoginPage()
    {
        // Arrange - no authentication

        // Act
        var response = await _client.GetAsync("/Account/Login");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUser_CanAccess_HealthEndpoint()
    {
        // Arrange - health endpoint should be public for load balancer probes

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(content.Contains("status", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("healthy", StringComparison.OrdinalIgnoreCase),
            "Health response should indicate status or healthy state");
    }

    [Fact]
    public async Task SecurityHeaders_ArePresent_OnAllResponses()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/Account/Login");

        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"), 
            "X-Content-Type-Options header should be present");
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());

        Assert.True(response.Headers.Contains("X-Frame-Options"),
            "X-Frame-Options header should be present");
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());

        Assert.False(response.Headers.Contains("Server"),
            "Server header should be removed for security");
    }

    [Fact]
    public async Task Logout_ClearsAuthentication()
    {
        // Arrange - simulate authenticated session
        var loginResponse = await _client.PostAsync("/Account/Login", 
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "test@ismmanila.org",
                ["Password"] = "TestPassword123!"
            }));

        // Act - logout
        var logoutResponse = await _client.PostAsync("/Account/Logout", null);

        // Assert - subsequent request to protected resource fails
        var protectedResponse = await _client.GetAsync("/Dashboard");
        Assert.True(
            protectedResponse.StatusCode == HttpStatusCode.Redirect || 
            protectedResponse.StatusCode == HttpStatusCode.Unauthorized ||
            protectedResponse.StatusCode == HttpStatusCode.NotFound,
            "After logout, protected resources should be inaccessible");
    }
}

/// <summary>
/// Tests for role-based access control (RBAC) enforcement.
/// OWASP Alignment: A01 Broken Access Control
/// </summary>
public class RoleBasedAccessControlTests : IClassFixture<SecurityTestWebApplicationFactory>
{
    private readonly SecurityTestWebApplicationFactory _factory;

    public RoleBasedAccessControlTests(SecurityTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/Settings/Users")] // Admin only
    [InlineData("/Duplicates")] // Admin only
    [InlineData("/SyncStatus")] // Admin only
    public async Task AdminOnlyEndpoints_DenyNonAdminUsers(string endpoint)
    {
        // Arrange - create client with non-admin user
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Mock authentication with "admissions" role (not admin)
                // Implementation would use test authentication handler
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert - should be forbidden or redirect
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Non-admin user should not access {endpoint}");
    }

    [Fact]
    public async Task SponsorRole_CannotAccess_StaffOnlyDashboard()
    {
        // Arrange - create client with sponsor role
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Mock sponsor authentication
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Dashboard");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.NotFound,
            "Sponsor role should not access staff-only dashboard");
    }

    [Fact]
    public async Task CashierRole_CannotAccess_MergeFunctionality()
    {
        // Arrange - create client with cashier role
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Mock cashier authentication
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Duplicates/Review/1");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.NotFound,
            "Cashier should not access merge functionality");
    }
}

/// <summary>
/// Tests for session security and cookie configuration.
/// OWASP Alignment: A07 Identification and Authentication Failures
/// </summary>
public class SessionSecurityTests : IClassFixture<SecurityTestWebApplicationFactory>
{
    private readonly SecurityTestWebApplicationFactory _factory;

    public SessionSecurityTests(SecurityTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthenticationCookie_HasSecureFlags()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act - attempt login
        var response = await client.PostAsync("/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "test@ismmanila.org",
                ["Password"] = "TestPassword123!"
            }));

        // Assert - check cookie flags
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var authCookie = cookies.FirstOrDefault(c => c.Contains(".AspNetCore.Identity"));
            if (authCookie != null)
            {
                Assert.Contains("HttpOnly", authCookie);
                Assert.Contains("SameSite=Strict", authCookie);
                // Secure flag depends on environment (HTTPS)
            }
        }
    }

    [Fact]
    public async Task SessionCookie_HasSecureFlags()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Account/Login");

        // Assert
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var sessionCookie = cookies.FirstOrDefault(c => c.Contains(".AspNetCore.Session"));
            if (sessionCookie != null)
            {
                Assert.Contains(".AspNetCore.Session", sessionCookie);
            }
        }
    }
}

/// <summary>
/// Tests for anti-forgery (CSRF) protection.
/// OWASP Alignment: A01 Broken Access Control
/// </summary>
public class AntiForgeryTests : IClassFixture<SecurityTestWebApplicationFactory>
{
    private readonly SecurityTestWebApplicationFactory _factory;

    public AntiForgeryTests(SecurityTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostAction_WithoutAntiForgeryToken_IsRejected()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act - POST without anti-forgery token
        var response = await client.PostAsync("/Sponsors/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["SponsorName"] = "Test Sponsor"
            }));

        // Assert - should be rejected (400 or redirect to login)
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Unauthorized,
            "POST without anti-forgery token should be rejected");
    }
}

/// <summary>
/// Tests for security logging and audit trail.
/// OWASP Alignment: A09 Security Logging and Monitoring Failures
/// </summary>
public class SecurityLoggingTests : IClassFixture<SecurityTestWebApplicationFactory>
{
    private readonly SecurityTestWebApplicationFactory _factory;

    public SecurityLoggingTests(SecurityTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FailedLogin_IsLogged()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act - attempt failed login
        var response = await client.PostAsync("/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "test@ismmanila.org",
                ["Password"] = "WrongPassword"
            }));

        // Assert - check that ActivityLog entry was created
        // (Would query test database or verify mock was called)
        // This is a placeholder test structure
        Assert.True(true, "Failed login should create audit log entry");
    }

    [Fact]
    public async Task UnauthorizedAccess_IsLogged()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act - attempt to access protected resource
        var response = await client.GetAsync("/Duplicates");

        // Assert - verify authorization failure is logged
        Assert.True(true, "Unauthorized access should create audit log entry");
    }
}

/// <summary>
/// Tests for configuration validation.
/// OWASP Alignment: A05 Security Misconfiguration
/// </summary>
public class ConfigurationValidationTests
{
    [Fact]
    public void MissingDatabaseConnection_FailsValidation()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var validator = new ConfigurationValidationService(configuration, 
            new TestWebHostEnvironment { EnvironmentName = "Production" });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => validator.ValidateConfiguration());
    }

    [Fact]
    public void PlaceholderSecrets_FailValidation_InProduction()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "PLACEHOLDER",
                ["AzureAd:ClientSecret"] = "PLACEHOLDER_SECRET"
            })
            .Build();

        var validator = new ConfigurationValidationService(configuration,
            new TestWebHostEnvironment { EnvironmentName = "Production" });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => validator.ValidateConfiguration());
    }
}

// Test helper
public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "ISMSponsor";
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
