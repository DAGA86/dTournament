using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record MatchDto(
    Guid Id,
    Guid AgeGroupId,
    Guid HomeTeamId,
    string HomeTeamName,
    Guid AwayTeamId,
    string AwayTeamName,
    int RoundNumber,
    DateTimeOffset? ScheduledStartUtc,
    string? VenueName,
    int PlannedDurationMinutes,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    CompetitionPhase Phase = CompetitionPhase.League);
