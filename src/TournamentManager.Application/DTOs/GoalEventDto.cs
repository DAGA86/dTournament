namespace TournamentManager.Application.DTOs;

public sealed record GoalEventDto(Guid Id, Guid MatchId, string TeamName, string PlayerName, int MatchMinute, int MatchPeriodNumber, string MatchMinuteDisplay, bool IsOwnGoal, bool IsActive, DateTimeOffset RecordedAtUtc);
