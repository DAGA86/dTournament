using TournamentManager.Domain.Enums;

namespace TournamentManager.Domain.Entities;

public sealed class KnockoutAssignment : BaseEntity
{
    public Guid AgeGroupId { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public CompetitionPhase Phase { get; set; }
    public int SlotNumber { get; set; }
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }
    public string Source { get; set; } = string.Empty;
}
