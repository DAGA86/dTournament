namespace TournamentManager.Domain.Entities;

public sealed class Player : BaseEntity
{
    public const int FullNameMaxLength = 180;
    public const int DisplayNameMaxLength = 80;
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public int? ShirtNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AgeOverrideApproved { get; set; }
    public string? AgeOverrideApprovedByUserId { get; set; }
    public string? AgeOverrideReason { get; set; }

    public void Validate(AgeGroup ageGroup)
    {
        if (string.IsNullOrWhiteSpace(FullName)) throw new InvalidOperationException("Player full name is required.");
        if (string.IsNullOrWhiteSpace(DisplayName)) throw new InvalidOperationException("Player display name is required.");
        if (BirthDate > DateOnly.FromDateTime(DateTime.UtcNow)) throw new InvalidOperationException("Player birth date cannot be in the future.");
        if (ShirtNumber is < 1 or > 999) throw new InvalidOperationException("Shirt number must be between 1 and 999.");
        var birthYear = BirthDate.Year;
        var outsideRange = (ageGroup.BirthYearFrom.HasValue && birthYear < ageGroup.BirthYearFrom.Value) || (ageGroup.BirthYearTo.HasValue && birthYear > ageGroup.BirthYearTo.Value);
        if (outsideRange && !AgeOverrideApproved) throw new InvalidOperationException("Player birth year is outside the age group limits.");
    }
}