using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Tournaments;

public sealed class TournamentCreateViewModel
{
    [Required, MaxLength(160)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [Required, MaxLength(160)] public string Location { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)] public DateOnly StartsOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    [Required, DataType(DataType.Date)] public DateOnly EndsOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    [MaxLength(4000)] public string? RulesNotes { get; set; }
}