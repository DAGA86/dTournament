namespace TournamentManager.Domain.Entities;

public sealed class Group : BaseEntity
{
    public const int NameMaxLength = 80;
    public Guid AgeGroupId { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
