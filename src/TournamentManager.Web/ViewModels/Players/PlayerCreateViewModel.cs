using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Web.ViewModels.Players;

public sealed class PlayerCreateViewModel
{
    [Required] public Guid TeamId { get; set; }
    [Required, MaxLength(180)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string DisplayName { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)] public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
    [Range(1, 999)] public int? ShirtNumber { get; set; }
    public bool AgeOverrideApproved { get; set; }
    [MaxLength(1000)] public string? AgeOverrideReason { get; set; }
}