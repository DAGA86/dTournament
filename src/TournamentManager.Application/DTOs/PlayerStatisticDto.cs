namespace TournamentManager.Application.DTOs;

public sealed record PlayerStatisticDto(Guid PlayerId, string PlayerName, string TeamName, int? ShirtNumber, int Goals, int PlayerOfTheMatchVotes, int GoalkeeperOfTheMatchVotes);
