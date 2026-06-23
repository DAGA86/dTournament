using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.MatchManagement;

public sealed class RegisterGoalViewModel
{
    [Required] public Guid MatchId { get; set; }
    [Required] public Guid TeamId { get; set; }
    [Required] public Guid PlayerId { get; set; }
    [Range(1, 2)] public int PeriodNumber { get; set; } = 1;
    public bool IsOwnGoal { get; set; }
}
