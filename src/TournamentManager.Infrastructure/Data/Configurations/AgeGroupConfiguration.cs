using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class AgeGroupConfiguration : IEntityTypeConfiguration<AgeGroup>
{
    public void Configure(EntityTypeBuilder<AgeGroup> builder)
    {
        builder.ToTable("AgeGroups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AgeGroup.NameMaxLength);
        builder.Property(x => x.TieBreakerOrder).IsRequired().HasMaxLength(256);
        builder.HasOne(x => x.Tournament).WithMany(x => x.AgeGroups).HasForeignKey(x => x.TournamentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TournamentId, x.Name }).IsUnique();
    }
}
