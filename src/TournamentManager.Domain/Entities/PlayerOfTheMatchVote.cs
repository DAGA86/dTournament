namespace TournamentManager.Domain.Entities;

public sealed class PlayerOfTheMatchVote : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match? Match { get; set; }
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }
    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }
    public string SelectedByUserId { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Validate()
    {
        if (TeamId == Guid.Empty) throw new InvalidOperationException("Vote team is required.");
        if (PlayerId == Guid.Empty) throw new InvalidOperationException("Vote player is required.");
        if (string.IsNullOrWhiteSpace(SelectedByUserId)) throw new InvalidOperationException("Selected by user is required.");
    }
}
