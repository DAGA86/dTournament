using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(Player.FullNameMaxLength);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(Player.DisplayNameMaxLength);
        builder.Property(x => x.AgeOverrideApprovedByUserId).HasMaxLength(450);
        builder.Property(x => x.AgeOverrideReason).HasMaxLength(1000);
        builder.HasOne(x => x.Team).WithMany(x => x.Players).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TeamId, x.ShirtNumber }).IsUnique().HasFilter("[ShirtNumber] IS NOT NULL");
        builder.HasIndex(x => x.DisplayName);
    }
}
