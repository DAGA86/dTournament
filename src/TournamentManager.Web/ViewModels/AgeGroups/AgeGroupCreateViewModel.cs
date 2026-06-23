using System.ComponentModel.DataAnnotations;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Web.ViewModels.AgeGroups;

public sealed class AgeGroupCreateViewModel
{
    [Required] public Guid TournamentId { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    public int? BirthYearFrom { get; set; }
    public int? BirthYearTo { get; set; }
    [Range(1, 180)] public int MatchDurationMinutes { get; set; } = 20;
    public CompetitionFormat CompetitionFormat { get; set; }
}