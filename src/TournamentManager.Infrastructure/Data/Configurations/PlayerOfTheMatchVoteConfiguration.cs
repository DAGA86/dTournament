using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class PlayerOfTheMatchVoteConfiguration : IEntityTypeConfiguration<PlayerOfTheMatchVote>
{
    public void Configure(EntityTypeBuilder<PlayerOfTheMatchVote> builder)
    {
        builder.ToTable("PlayerOfTheMatchVotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SelectedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Match).WithMany(x => x.PlayerOfTheMatchVotes).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MatchId, x.TeamId }).IsUnique();
    }
}
