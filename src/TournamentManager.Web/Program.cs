using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TournamentManager.Application.Services;
using TournamentManager.Infrastructure;
using TournamentManager.Infrastructure.Data;
using TournamentManager.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute())).AddViewLocalization().AddDataAnnotationsLocalization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<TournamentService>();
builder.Services.AddScoped<AgeGroupValidationService>();
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
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

var app = builder.Build();
var supportedCultures = new[] { new CultureInfo("pt-PT") };
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("pt-PT"), SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures });
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
await IdentitySeeder.SeedFirstAdministratorAsync(app.Services, app.Configuration, app.Logger);
app.Run();
