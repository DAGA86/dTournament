using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class AuditService(IAuditLogRepository auditLogRepository)
{
    public async Task RecordAsync(string userId, string operationType, string entityName, string entityId, string? previousValues, string? newValues, string? reason = null, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog { UserId = userId, OperationType = operationType, EntityName = entityName, EntityId = entityId, PreviousValues = previousValues, NewValues = newValues, Reason = reason };
        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await auditLogRepository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<AuditLog>> ListRecentAsync(int count, CancellationToken cancellationToken = default) => auditLogRepository.ListRecentAsync(count, cancellationToken);
}
