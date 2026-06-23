using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Teams;

public sealed class TeamCreateViewModel
{
    [Required] public Guid AgeGroupId { get; set; }
    [Required, MaxLength(160)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(32)] public string ShortName { get; set; } = string.Empty;
    [Required, MaxLength(160)] public string Club { get; set; } = string.Empty;
    [Required, MaxLength(160)] public string ResponsiblePerson { get; set; } = string.Empty;
    [MaxLength(160)] public string? Contact { get; set; }
    [MaxLength(16)] public string? PrimaryColor { get; set; }
}
