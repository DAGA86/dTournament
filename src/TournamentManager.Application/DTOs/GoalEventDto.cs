namespace TournamentManager.Application.DTOs;

public sealed record GoalEventDto(Guid Id, Guid MatchId, string TeamName, string PlayerName, int MatchMinute, bool IsOwnGoal, bool IsActive, DateTimeOffset RecordedAtUtc);
