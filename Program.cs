using ISMSponsor.Data;
using ISMSponsor.Models;
using ISMSponsor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("DefaultConnection not found in configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 1;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// register our services
builder.Services.AddScoped<SchoolYearService>();
builder.Services.AddScoped<LogsService>();
builder.Services.AddScoped<SponsorService>();
builder.Services.AddScoped<ChangeRequestService>();
builder.Services.AddScoped<AdminUserService>();

builder.Services.AddTransient<DbInitializer>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// initialize database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// populate school years for layout
app.Use(async (context, next) =>
{
    var svc = context.RequestServices.GetService<ISMSponsor.Services.SchoolYearService>();
    if (svc != null)
    {
        var years = await svc.GetAllAsync();
        context.Items["SchoolYears"] = years;
        // ensure session active year set
        if (string.IsNullOrEmpty(context.Session.GetString("ActiveSchoolYear")))
        {
            var active = years.FirstOrDefault(y => y.IsActive);
            if (active != null)
                context.Session.SetString("ActiveSchoolYear", active.SchoolYearId);
        }
    }
    await next();
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

app.Run();
