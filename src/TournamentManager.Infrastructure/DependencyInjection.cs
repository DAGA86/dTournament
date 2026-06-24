using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TournamentManager.Application.Abstractions;
using TournamentManager.Infrastructure.Data;

namespace TournamentManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITournamentRepository, TournamentRepository>();
        services.AddScoped<IAgeGroupRepository, AgeGroupRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<ITeamStaffMemberRepository, TeamStaffMemberRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        return services;
    }
}
