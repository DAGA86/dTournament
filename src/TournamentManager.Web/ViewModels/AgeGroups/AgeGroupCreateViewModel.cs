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
    [Range(1, 2)] public int NumberOfPeriods { get; set; } = 1;
    [Range(0, 60)] public int HalfTimeBreakMinutes { get; set; }
    [Range(1, 26)] public int GroupCount { get; set; } = 2;
    [Range(1, 16)] public int AdvancingTeamsPerGroup { get; set; } = 2;
    public CompetitionPhase FinalsStartPhase { get; set; } = CompetitionPhase.SemiFinal;
    public CompetitionFormat CompetitionFormat { get; set; }
}
