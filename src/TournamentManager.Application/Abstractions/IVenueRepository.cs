using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface IVenueRepository
{
    Task<IReadOnlyList<Venue>> ListAsync(CancellationToken cancellationToken = default);
    Task<Venue?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludingVenueId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Venue venue, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
