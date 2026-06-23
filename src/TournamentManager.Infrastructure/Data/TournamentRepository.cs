using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class TournamentRepository(ApplicationDbContext dbContext) : ITournamentRepository
{
    public async Task<IReadOnlyList<Tournament>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Tournaments.Include(x => x.AgeGroups).OrderByDescending(x => x.StartsOn).ToListAsync(cancellationToken);

    public Task<Tournament?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Tournaments.Include(x => x.AgeGroups).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Tournament tournament, CancellationToken cancellationToken = default) => await dbContext.Tournaments.AddAsync(tournament, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
