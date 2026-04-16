using ISMSponsor.Data;
using ISMSponsor.Models;
using ISMSponsor.Services;
using ISMSponsor.Integration.Adapters;
using ISMSponsor.Integration.Orchestration.Services;
using ISMSponsor.Middleware;
using ISMSponsor.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using System.Linq;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// AZURE KEY VAULT INTEGRATION (Production-Safe Secret Management)
// ============================================================================
// Load secrets from Azure Key Vault in non-Development environments
// Key Vault name is configured via environment variable or appsettings
if (!builder.Environment.IsDevelopment())
{
    var keyVaultName = builder.Configuration["KeyVault:Name"];
    
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        
        // Use Managed Identity for authentication (recommended for Azure App Service)
        // Falls back to Azure CLI or Visual Studio credentials for local development
        builder.Configuration.AddAzureKeyVault(
            keyVaultUri,
            new DefaultAzureCredential());
        
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Azure Key Vault configuration loaded from {KeyVaultUri}", keyVaultUri);
    }
    else
    {
        // Log warning but don't fail - allows deployment without Key Vault in staging
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogWarning("KeyVault:Name not configured. Ensure secrets are provided via environment variables or App Service settings.");
    }
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("DefaultConnection not found in configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Step 7: Configure Identity with security hardening
var passwordConfig = builder.Configuration.GetSection("SponsorAuth:PasswordRequirements");
var lockoutConfig = builder.Configuration.GetSection("SponsorAuth:LockoutSettings");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password requirements from configuration
        options.Password.RequireDigit = passwordConfig.GetValue<bool>("RequireDigit", true);
        options.Password.RequireNonAlphanumeric = passwordConfig.GetValue<bool>("RequireNonAlphanumeric", true);
        options.Password.RequireUppercase = passwordConfig.GetValue<bool>("RequireUppercase", true);
        options.Password.RequiredLength = passwordConfig.GetValue<int>("RequiredLength", 8);
        options.User.RequireUniqueEmail = false;
        
        // Lockout settings
        options.Lockout.MaxFailedAccessAttempts = lockoutConfig.GetValue<int>("MaxFailedAccessAttempts", 5);
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
            lockoutConfig.GetValue<int>("LockoutDurationMinutes", 15));
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// ============================================================================
// GOOGLE OAUTH AUTHENTICATION
// ============================================================================
// Google Sign-In enabled for ISM staff/admins
// 
// SETUP INSTRUCTIONS:
// 1. Go to Google Cloud Console: https://console.cloud.google.com
// 2. Create a new project or select existing project
// 3. Enable Google+ API (or People API for newer projects)
// 4. Go to Credentials > Create Credentials > OAuth 2.0 Client ID
// 5. Set Application Type to "Web Application"
// 6. Add authorized redirect URIs:
//    - http://localhost:5001/Account/GoogleCallback (Development)
//    - https://yourdomain.com/Account/GoogleCallback (Production)
// 7. Copy Client ID and Client Secret
// 8. Update appsettings.Development.json with your credentials
// 
// SECURITY NOTES:
// - Only @ismanila.org emails are allowed (enforced in GoogleCallback)
// - Auto-provisioned users are assigned 'admin' role by default
// - Customize role assignment in AccountController.GoogleCallback if needed
// ============================================================================

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var googleConfig = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleConfig["ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
        options.ClientSecret = googleConfig["ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
        options.CallbackPath = "/Account/GoogleCallback";
        
        // Request email and profile scopes
        options.Scope.Add("email");
        options.Scope.Add("profile");
        
        // Save tokens for potential future use
        options.SaveTokens = true;
    });

// Secure cookie configuration
var cookieSecurePolicy = builder.Configuration["Security:CookieSecurePolicy"];
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy switch
    {
        "Always" => CookieSecurePolicy.Always,
        "None" => CookieSecurePolicy.None,
        _ => CookieSecurePolicy.SameAsRequest
    };
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy switch
    {
        "Always" => CookieSecurePolicy.Always,
        "None" => CookieSecurePolicy.None,
        _ => CookieSecurePolicy.SameAsRequest
    };
});

// register our services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SchoolYearService>();
builder.Services.AddScoped<SchoolYearContextService>();
builder.Services.AddScoped<LogsService>();
builder.Services.AddScoped<SponsorService>();
builder.Services.AddScoped<ChangeRequestService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<LetterOfGuaranteeService>();
builder.Services.AddScoped<CoverageEvaluationService>();
builder.Services.AddScoped<SponsorChangeRequestService>();
builder.Services.AddScoped<AuditService>();

// Step 6: Duplicate detection, merge, and integration services
builder.Services.AddScoped<DuplicateDetectionService>();
builder.Services.AddScoped<SponsorMergeService>();
builder.Services.AddScoped<IIntegrationSyncService, IntegrationSyncService>();
builder.Services.AddScoped<IPowerSchoolAdapter, MockPowerSchoolAdapter>();
builder.Services.AddScoped<IStudentChargingPortalAdapter, MockStudentChargingPortalAdapter>();
builder.Services.AddScoped<INetSuiteAdapter, MockNetSuiteAdapter>();
builder.Services.AddScoped<IOnlineBillingSystemAdapter, MockOnlineBillingSystemAdapter>();

// PowerSchool Sponsor_OrgName list publishing service (Manuscript requirement)
builder.Services.AddScoped<IPowerSchoolSponsorListService, PowerSchoolSponsorListService>();

// Downstream Integration Orchestration (Comprehensive multi-target synchronization)
builder.Services.AddScoped<INetSuiteIntegrationService, NetSuiteIntegrationService>();
builder.Services.AddScoped<IObsIntegrationService, ObsIntegrationService>();
builder.Services.AddScoped<IScpIntegrationService, ScpIntegrationService>();
builder.Services.AddScoped<IIntegrationOrchestrator, IntegrationOrchestrator>();

// Step 7: Security and operational services
builder.Services.AddSingleton<ConfigurationValidationService>();
builder.Services.AddScoped<SecurityAuditService>();

// Step 8: Reports, monitoring, and pilot support services
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<MonitoringService>();
builder.Services.AddScoped<SmokeTestService>();
builder.Services.AddScoped<FeedbackService>();

// Step 9: Demo data seeder
builder.Services.AddScoped<DemoDataSeeder>();

// Step 7: Health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready", "live" })
    .AddCheck<ConfigurationHealthCheck>("configuration", tags: new[] { "ready" })
    .AddCheck<SyncHealthCheck>("sync", tags: new[] { "ready" })
    .AddCheck<AuditHealthCheck>("audit", tags: new[] { "ready" });

// Step 7: Application Insights (mandatory in non-Development environments)
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}
else if (!builder.Environment.IsDevelopment())
{
    // Fail fast if Application Insights not configured in production/pilot
    throw new InvalidOperationException(
        "ApplicationInsights:ConnectionString is required for non-Development environments. " +
        "Configure in Azure Key Vault or App Service settings.");
}

// Step 7: Anti-forgery configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy switch
    {
        "Always" => CookieSecurePolicy.Always,
        "None" => CookieSecurePolicy.None,
        _ => CookieSecurePolicy.SameAsRequest
    };
});

// Step 7: Configure forwarded headers for Azure App Service / load balancers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddTransient<DbInitializer>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Step 8: Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ISM Sponsor API",
        Version = "v1",
        Description = "REST API for Student Charging Portal, PowerSchool, NetSuite, and OBS integration",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ISM Development Team",
            Email = "support@ismsponsor.edu"
        }
    });

    // Include XML documentation comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Configure JWT Bearer authentication for Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Step 7: Validate configuration on startup (fail fast if misconfigured)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var configValidation = scope.ServiceProvider.GetRequiredService<ConfigurationValidationService>();
        try
        {
            configValidation.ValidateConfiguration();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("[OK] Configuration validation passed");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogCritical(ex, "[FAIL] Configuration validation failed. Application cannot start safely.");
            throw; // Fail fast - do not start with invalid configuration
        }
    }
}

// ============================================================================
// DATABASE INITIALIZATION (Safe for Production)
// ============================================================================
// Initialize database ONLY in Development or if explicitly enabled
// Production/Pilot migrations should run via deployment pipeline
if (!app.Environment.IsEnvironment("Testing"))
{
    var runMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup", false);
    
    if (app.Environment.IsDevelopment() || runMigrationsOnStartup)
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Running database initialization (environment: {Environment})", app.Environment.EnvironmentName);
            
            var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            initializer.Initialize();
            
            logger.LogInformation("[OK] Database initialization complete");
        }
    }
    else
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Skipping database migration on startup (production-safe mode). Migrations should be run via deployment pipeline.");
        }
    }
}

// Step 7: Configure forwarded headers (must be before other middleware)
app.UseForwardedHeaders();

// Step 7: Global exception handler (must be early in pipeline)
app.UseGlobalExceptionHandler();

// Step 7: Security headers middleware
app.UseSecurityHeaders();

// Step 8: Enable Swagger in Development and Pilot environments
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Pilot"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ISM Sponsor API v1");
        options.RoutePrefix = "api/docs"; // Access at /api/docs
    });
}

if (!app.Environment.IsDevelopment())
{
    var useHttps = builder.Configuration.GetValue<bool>("Security:UseHttpsRedirection");
    var useHsts = builder.Configuration.GetValue<bool>("Security:UseHsts");
    
    if (useHsts)
    {
        app.UseHsts();
    }
    
    if (useHttps)
    {
        app.UseHttpsRedirection();
    }
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ensure default active school year is set in session
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("ActiveSchoolYear")))
    {
        var svc = context.RequestServices.GetService<ISMSponsor.Services.SchoolYearService>();
        if (svc != null)
        {
            var active = (await svc.GetAllAsync()).FirstOrDefault(y => y.IsActive);
            if (active != null)
                context.Session.SetString("ActiveSchoolYear", active.SchoolYearId);
        }
    }
    await next();
});

// Step 7: Map health check endpoints with granular probes
// /health - Overall health (for monitoring dashboards)
// /health/ready - Readiness probe (database + config + services ready to accept traffic)
// /health/live - Liveness probe (process is running, lightweight check)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        }));
    }
});

app.MapControllerRoute(
    name: "settings",
    pattern: "Settings/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "sponsor-alias",
    pattern: "Sponsor/{action=Profile}/{id?}",
    defaults: new { controller = "Sponsors" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Handle favicon.ico requests
app.MapGet("/favicon.ico", async context =>
{
    context.Response.Redirect("/icons/icon-192.png", permanent: true);
});

app.Run();

// Make Program class accessible for testing
public partial class Program { }
