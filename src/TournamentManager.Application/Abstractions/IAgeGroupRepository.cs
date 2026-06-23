using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface IAgeGroupRepository
{
    Task<AgeGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgeGroup>> ListByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
    Task AddAsync(AgeGroup ageGroup, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
