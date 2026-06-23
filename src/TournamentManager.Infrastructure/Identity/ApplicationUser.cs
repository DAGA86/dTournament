using Microsoft.AspNetCore.Identity;

namespace TournamentManager.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
