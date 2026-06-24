namespace TournamentManager.Domain.Entities;

public sealed class TeamStaffMember : BaseEntity
{
    public const int FullNameMaxLength = 180;
    public const int RoleMaxLength = 80;

    public Guid TeamId { get; set; }
    public Team? Team { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(FullName)) throw new InvalidOperationException("Staff member name is required.");
        if (string.IsNullOrWhiteSpace(Role)) throw new InvalidOperationException("Staff member role is required.");
    }
}