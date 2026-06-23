using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record MatchDto(Guid Id, Guid AgeGroupId, string HomeTeamName, string AwayTeamName, int RoundNumber, DateTimeOffset? ScheduledStartUtc, string? VenueName, int PlannedDurationMinutes, MatchStatus Status);
