namespace TournamentManager.Domain.Entities;

public sealed class Venue : BaseEntity
{
    public const int NameMaxLength = 120;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new InvalidOperationException("Venue name is required.");
    }
}
