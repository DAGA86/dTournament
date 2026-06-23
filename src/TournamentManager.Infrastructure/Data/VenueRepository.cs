using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class VenueRepository(ApplicationDbContext dbContext) : IVenueRepository
{
    public async Task<IReadOnlyList<Venue>> ListAsync(CancellationToken cancellationToken = default) => await dbContext.Venues.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    public Task<Venue?> GetAsync(Guid id, CancellationToken cancellationToken = default) => dbContext.Venues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> ExistsByNameAsync(string name, Guid? excludingVenueId = null, CancellationToken cancellationToken = default) => dbContext.Venues.AnyAsync(x => x.Name == name && (!excludingVenueId.HasValue || x.Id != excludingVenueId.Value), cancellationToken);
    public async Task AddAsync(Venue venue, CancellationToken cancellationToken = default) => await dbContext.Venues.AddAsync(venue, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}