using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TournamentManager.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedFirstAdministratorAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { "Administrator", "Operator", "Viewer" })
        {
            if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
        }
        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        if (await userManager.FindByEmailAsync(email) is not null) return;
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "Initial Administrator" };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) { logger.LogWarning("Initial administrator could not be created: {Errors}", string.Join("; ", result.Errors.Select(x => x.Description))); return; }
        await userManager.AddToRoleAsync(user, "Administrator");
        logger.LogInformation("Initial administrator was created from secure configuration.");
    }
}