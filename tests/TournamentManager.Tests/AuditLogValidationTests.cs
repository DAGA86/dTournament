using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class AuditLogValidationTests
{
    [Fact]
    public void AuditLog_Defaults_OccurredAtUtc()
    {
        var auditLog = new AuditLog { UserId = "user", OperationType = "Operation", EntityName = "Entity", EntityId = "1" };
        Assert.True(auditLog.OccurredAtUtc <= DateTimeOffset.UtcNow);
    }
}
