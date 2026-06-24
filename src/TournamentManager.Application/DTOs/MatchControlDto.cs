using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record MatchControlDto(Guid Id, Guid HomeTeamId, Guid AwayTeamId, string HomeTeamName, string AwayTeamName, int HomeScore, int AwayScore, MatchStatus Status, DateTimeOffset? ActualStartUtc, DateTimeOffset? ActualEndUtc, long CurrentElapsedSeconds, int CurrentPeriodNumber, int PlannedDurationMinutes, int PlannedPeriodCount, int PlannedPeriodDurationMinutes, IReadOnlyList<GoalEventDto> Goals, IReadOnlyList<PlayerSelectionDto> HomePlayers, IReadOnlyList<PlayerSelectionDto> AwayPlayers, Guid? GoalkeeperOfTheMatchPlayerId);
