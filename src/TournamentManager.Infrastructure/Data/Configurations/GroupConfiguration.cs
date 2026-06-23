using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(Group.NameMaxLength);
        builder.HasOne(x => x.AgeGroup).WithMany(x => x.Groups).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.AgeGroupId, x.Name }).IsUnique();
    }
}
