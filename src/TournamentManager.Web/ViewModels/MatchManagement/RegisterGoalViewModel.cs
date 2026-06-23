using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.MatchManagement;

public sealed class RegisterGoalViewModel
{
    [Required] public Guid MatchId { get; set; }
    [Required] public Guid TeamId { get; set; }
    [Required] public Guid PlayerId { get; set; }
    [Range(0, 300)] public int MatchMinute { get; set; }
    public bool IsOwnGoal { get; set; }
}
