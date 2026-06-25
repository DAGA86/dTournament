using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class AgeGroupRepository(ApplicationDbContext dbContext) : IAgeGroupRepository
{
    public Task<AgeGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default) => dbContext.AgeGroups.Include(x => x.Tournament).Include(x => x.Groups).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyList<AgeGroup>> ListByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => await dbContext.AgeGroups.Where(x => x.TournamentId == tournamentId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    public async Task AddAsync(AgeGroup ageGroup, CancellationToken cancellationToken = default) => await dbContext.AgeGroups.AddAsync(ageGroup, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
