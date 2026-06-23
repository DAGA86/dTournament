using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class GoalEventConfiguration : IEntityTypeConfiguration<GoalEvent>
{
    public void Configure(EntityTypeBuilder<GoalEvent> builder)
    {
        builder.ToTable("GoalEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecordedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Match).WithMany(x => x.GoalEvents).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MatchId, x.RecordedAtUtc });
    }
}
