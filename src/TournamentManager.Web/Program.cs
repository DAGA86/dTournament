using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.RateLimiting;
using TournamentManager.Application.Services;
using TournamentManager.Infrastructure;
using TournamentManager.Infrastructure.Data;
using TournamentManager.Infrastructure.Identity;
using TournamentManager.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute())).AddViewLocalization().AddDataAnnotationsLocalization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<TournamentService>();
builder.Services.AddScoped<AgeGroupValidationService>();
builder.Services.AddScoped<AgeGroupService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<TeamRosterSubmissionService>();
builder.Services.AddScoped<VenueService>();
builder.Services.AddScoped<ScheduleGenerationService>();
builder.Services.AddScoped<MatchScheduleService>();
builder.Services.AddScoped<MatchManagementService>();
builder.Services.AddScoped<StandingsCalculationService>();
builder.Services.AddScoped<PlayerStatisticsService>();
builder.Services.AddScoped<QualificationService>();
builder.Services.AddScoped<KnockoutProgressionService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders().AddDefaultUI();
builder.Services.ConfigureApplicationCookie(options => { options.Cookie.HttpOnly = true; options.Cookie.SecurePolicy = CookieSecurePolicy.Always; options.LoginPath = "/Identity/Account/Login"; });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("OperatorOrAdministrator", policy => policy.RequireRole("Operator", "Administrator"));
    options.AddPolicy("AuthenticatedViewer", policy => policy.RequireRole("Viewer", "Operator", "Administrator"));
});
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("Login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "anonymous", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
});
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

var app = builder.Build();
var supportedCultures = new[] { new CultureInfo("pt-PT") };
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("pt-PT"), SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures });
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<TournamentUpdatesHub>("/hubs/tournament-updates");
await IdentitySeeder.SeedFirstAdministratorAsync(app.Services, app.Configuration, app.Logger);
app.Run();
