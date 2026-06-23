namespace TournamentManager.Application.DTOs;

public sealed record PlayerDto(Guid Id, Guid TeamId, string TeamName, string FullName, string DisplayName, DateOnly BirthDate, int? ShirtNumber, bool IsActive, bool AgeOverrideApproved);