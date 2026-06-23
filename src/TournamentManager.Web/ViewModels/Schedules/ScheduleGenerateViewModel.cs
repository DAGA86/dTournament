using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Schedules;

public sealed class ScheduleGenerateViewModel
{
    [Required] public Guid AgeGroupId { get; set; }
    [Required] public DateTimeOffset FirstKickoffUtc { get; set; } = DateTimeOffset.UtcNow;
    [Range(1, 1440)] public int MinutesBetweenMatches { get; set; } = 30;
    public Guid? VenueId { get; set; }
}
