using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class TeamRepository(ApplicationDbContext dbContext) : ITeamRepository
{
    public async Task<IReadOnlyList<Team>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => await dbContext.Teams.Include(x => x.AgeGroup).Include(x => x.Players).Where(x => x.AgeGroupId == ageGroupId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    public Task<Team?> GetAsync(Guid id, CancellationToken cancellationToken = default) => dbContext.Teams.Include(x => x.AgeGroup).Include(x => x.Players).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> ExistsInAgeGroupAsync(Guid ageGroupId, string name, Guid? excludingTeamId = null, CancellationToken cancellationToken = default) => dbContext.Teams.AnyAsync(x => x.AgeGroupId == ageGroupId && x.Name == name && (!excludingTeamId.HasValue || x.Id != excludingTeamId.Value), cancellationToken);
    public async Task AddAsync(Team team, CancellationToken cancellationToken = default) => await dbContext.Teams.AddAsync(team, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
