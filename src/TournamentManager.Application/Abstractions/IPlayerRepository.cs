using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface IPlayerRepository
{
    Task<IReadOnlyList<Player>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Player?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ShirtNumberExistsAsync(Guid teamId, int shirtNumber, Guid? excludingPlayerId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Player player, CancellationToken cancellationToken = default);
    Task ReplaceForTeamAsync(Guid teamId, IReadOnlyList<Player> players, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
