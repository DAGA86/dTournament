using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface ITeamStaffMemberRepository
{
    Task<IReadOnlyList<TeamStaffMember>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task ReplaceForTeamAsync(Guid teamId, IReadOnlyList<TeamStaffMember> staffMembers, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}