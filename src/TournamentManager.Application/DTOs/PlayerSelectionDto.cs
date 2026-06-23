namespace TournamentManager.Application.DTOs;

public sealed record PlayerSelectionDto(Guid Id, Guid TeamId, string DisplayName, int? ShirtNumber);
