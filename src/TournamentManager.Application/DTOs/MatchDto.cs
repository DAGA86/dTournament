using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record MatchGoalDto(string TeamName, string PlayerName, int MatchMinute, int MatchPeriodNumber, string MatchMinuteDisplay, bool IsOwnGoal, bool IsActive);

public sealed record MatchDto(
    Guid Id,
    Guid AgeGroupId,
    Guid? HomeTeamId,
    string HomeTeamName,
    Guid? AwayTeamId,
    string AwayTeamName,
    int RoundNumber,
    DateTimeOffset? ScheduledStartUtc,
    string? VenueName,
    Guid? GroupId,
    string? GroupName,
    int PlannedDurationMinutes,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    int? HomePenaltyScore,
    int? AwayPenaltyScore,
    CompetitionPhase Phase = CompetitionPhase.League,
    int? CurrentMatchMinute = null,
    string? CurrentMatchMinuteDisplay = null,
    int CurrentPeriodNumber = 1,
    int PlannedPeriodCount = 1,
    IReadOnlyList<MatchGoalDto>? Goals = null);
