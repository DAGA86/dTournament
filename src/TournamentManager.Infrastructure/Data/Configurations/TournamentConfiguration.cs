using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(Tournament.NameMaxLength);
        builder.Property(x => x.Location).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.LogoPath).HasMaxLength(512);
        builder.Property(x => x.RulesNotes).HasMaxLength(4000);
        builder.HasIndex(x => x.Name);
    }
}