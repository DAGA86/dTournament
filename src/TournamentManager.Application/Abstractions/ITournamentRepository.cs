using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface ITournamentRepository
{
    Task<IReadOnlyList<Tournament>> ListAsync(CancellationToken cancellationToken = default);
    Task<Tournament?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Tournament tournament, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}