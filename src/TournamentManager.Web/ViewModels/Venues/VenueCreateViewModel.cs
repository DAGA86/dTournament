using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Venues;

public sealed class VenueCreateViewModel
{
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
}