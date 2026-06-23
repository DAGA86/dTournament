namespace TournamentManager.Domain.Entities;

public sealed class Team : BaseEntity
{
    public const int NameMaxLength = 160;
    public const int ShortNameMaxLength = 32;
    public const int ClubMaxLength = 160;
    public Guid AgeGroupId { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? PrimaryColor { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Player> Players { get; set; } = new List<Player>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new InvalidOperationException("Team name is required.");
        if (string.IsNullOrWhiteSpace(ShortName)) throw new InvalidOperationException("Team short name is required.");
        if (string.IsNullOrWhiteSpace(Club)) throw new InvalidOperationException("Club is required.");
    }
}
