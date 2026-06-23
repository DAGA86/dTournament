using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class KnockoutAssignmentConfiguration : IEntityTypeConfiguration<KnockoutAssignment>
{
    public void Configure(EntityTypeBuilder<KnockoutAssignment> builder)
    {
        builder.ToTable("KnockoutAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(160);
        builder.HasOne(x => x.AgeGroup).WithMany(x => x.KnockoutAssignments).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.AgeGroupId, x.Phase, x.SlotNumber }).IsUnique();
    }
}
