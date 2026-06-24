using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class TeamStaffMemberRepository(ApplicationDbContext dbContext) : ITeamStaffMemberRepository
{
    public async Task<IReadOnlyList<TeamStaffMember>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        await dbContext.TeamStaffMembers.Where(x => x.TeamId == teamId).OrderBy(x => x.FullName).ToListAsync(cancellationToken);

    public async Task ReplaceForTeamAsync(Guid teamId, IReadOnlyList<TeamStaffMember> staffMembers, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TeamStaffMembers.Where(x => x.TeamId == teamId).ToListAsync(cancellationToken);
        dbContext.TeamStaffMembers.RemoveRange(existing);
        await dbContext.TeamStaffMembers.AddRangeAsync(staffMembers, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}