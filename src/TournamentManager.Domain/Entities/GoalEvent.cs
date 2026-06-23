using System.Text.RegularExpressions;

namespace TournamentManager.Domain.Entities;

public sealed class GoalEvent : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match? Match { get; set; }
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }
    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }
    public int MatchMinute { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public bool IsOwnGoal { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Validate()
    {
        if (MatchMinute < 0) throw new InvalidOperationException("Match minute cannot be negative.");
        if (TeamId == Guid.Empty) throw new InvalidOperationException("Goal team is required.");
        if (PlayerId == Guid.Empty) throw new InvalidOperationException("Goal player is required.");
        if (string.IsNullOrWhiteSpace(RecordedByUserId)) throw new InvalidOperationException("Recorded by user is required.");
    }
}
