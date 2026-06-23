using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record MatchControlDto(Guid Id, string HomeTeamName, string AwayTeamName, int HomeScore, int AwayScore, MatchStatus Status, DateTimeOffset? ActualStartUtc, DateTimeOffset? ActualEndUtc, IReadOnlyList<GoalEventDto> Goals);
