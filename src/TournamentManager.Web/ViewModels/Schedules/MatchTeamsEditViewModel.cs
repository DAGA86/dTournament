using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Schedules;

public sealed class MatchTeamsEditViewModel
{
    [Required] public Guid MatchId { get; set; }
    [Required] public Guid AgeGroupId { get; set; }
    [Required] public Guid HomeTeamId { get; set; }
    [Required] public Guid AwayTeamId { get; set; }
}
