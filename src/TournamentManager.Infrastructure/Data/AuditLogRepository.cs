using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default) => await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    public async Task<IReadOnlyList<AuditLog>> ListRecentAsync(int count, CancellationToken cancellationToken = default) => await dbContext.AuditLogs.OrderByDescending(x => x.OccurredAtUtc).Take(count).ToListAsync(cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
