using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface ITeamRepository
{
    Task<IReadOnlyList<Team>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default);
    Task<Team?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsInAgeGroupAsync(Guid ageGroupId, string name, Guid? excludingTeamId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
