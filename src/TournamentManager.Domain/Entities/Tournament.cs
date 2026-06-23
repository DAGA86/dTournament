using TournamentManager.Domain.Enums;

namespace TournamentManager.Domain.Entities;

public sealed class Tournament : BaseEntity
{
    public const int NameMaxLength = 160;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public string? LogoPath { get; set; }
    public string? RulesNotes { get; set; }
    public ICollection<AgeGroup> AgeGroups { get; set; } = new List<AgeGroup>();

    public void ValidateDates()
    {
        if (EndsOn < StartsOn) throw new InvalidOperationException("The tournament end date cannot be before the start date.");
    }
}
