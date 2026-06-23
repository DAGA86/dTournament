using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class PlayerRepository(ApplicationDbContext dbContext) : IPlayerRepository
{
    public async Task<IReadOnlyList<Player>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default) => await dbContext.Players.Include(x => x.Team).Where(x => x.TeamId == teamId).OrderBy(x => x.DisplayName).ToListAsync(cancellationToken);
    public Task<Player?> GetAsync(Guid id, CancellationToken cancellationToken = default) => dbContext.Players.Include(x => x.Team).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> ShirtNumberExistsAsync(Guid teamId, int shirtNumber, Guid? excludingPlayerId = null, CancellationToken cancellationToken = default) => dbContext.Players.AnyAsync(x => x.TeamId == teamId && x.ShirtNumber == shirtNumber && (!excludingPlayerId.HasValue || x.Id != excludingPlayerId.Value), cancellationToken);
    public async Task AddAsync(Player player, CancellationToken cancellationToken = default) => await dbContext.Players.AddAsync(player, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
