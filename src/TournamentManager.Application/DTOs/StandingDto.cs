namespace TournamentManager.Application.DTOs;

public sealed record StandingDto(Guid TeamId, string TeamName, int Position, int Played, int Wins, int Draws, int Losses, int GoalsFor, int GoalsAgainst, int GoalDifference, int Points);
