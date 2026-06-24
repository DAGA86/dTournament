using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data.Configurations;

public sealed class TeamStaffMemberConfiguration : IEntityTypeConfiguration<TeamStaffMember>
{
    public void Configure(EntityTypeBuilder<TeamStaffMember> builder)
    {
        builder.ToTable("TeamStaffMembers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(TeamStaffMember.FullNameMaxLength);
        builder.Property(x => x.Role).IsRequired().HasMaxLength(TeamStaffMember.RoleMaxLength);
        builder.HasOne(x => x.Team).WithMany(x => x.StaffMembers).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TeamId, x.FullName, x.Role });
    }
}