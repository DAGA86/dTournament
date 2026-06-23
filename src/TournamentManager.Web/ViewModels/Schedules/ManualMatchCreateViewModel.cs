using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Schedules;

public sealed class ManualMatchCreateViewModel
{
    [Required] public Guid AgeGroupId { get; set; }
    [Required] public Guid HomeTeamId { get; set; }
    [Required] public Guid AwayTeamId { get; set; }
    [Range(1, 999)] public int RoundNumber { get; set; } = 1;
    public DateTimeOffset? ScheduledStartUtc { get; set; }
    public Guid? VenueId { get; set; }
}
