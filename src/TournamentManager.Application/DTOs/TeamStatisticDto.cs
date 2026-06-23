namespace TournamentManager.Application.DTOs;

public sealed record TeamStatisticDto(Guid TeamId, string TeamName, int GoalsFor, int GoalsAgainst, int PlayerCount, int PlayedMatches);
