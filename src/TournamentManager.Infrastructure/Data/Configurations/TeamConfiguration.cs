using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(Team.NameMaxLength);
        builder.Property(x => x.ShortName).IsRequired().HasMaxLength(Team.ShortNameMaxLength);
        builder.Property(x => x.Club).IsRequired().HasMaxLength(Team.ClubMaxLength);
        builder.Property(x => x.LogoPath).HasMaxLength(512);
        builder.Property(x => x.PrimaryColor).HasMaxLength(16);
        builder.Property(x => x.ResponsiblePerson).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Contact).HasMaxLength(160);
        builder.HasOne(x => x.AgeGroup).WithMany(x => x.Teams).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Group).WithMany(x => x.Teams).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.AgeGroupId, x.Name }).IsUnique();
    }
}
