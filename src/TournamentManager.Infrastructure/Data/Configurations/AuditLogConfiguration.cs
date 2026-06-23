using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.OperationType).IsRequired().HasMaxLength(120);
        builder.Property(x => x.EntityName).IsRequired().HasMaxLength(160);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(120);
        builder.Property(x => x.PreviousValues).HasMaxLength(4000);
        builder.Property(x => x.NewValues).HasMaxLength(4000);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => new { x.EntityName, x.EntityId });
    }
}
