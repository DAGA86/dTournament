namespace TournamentManager.Application.DTOs;

public sealed record GeneratedMatchDto(Guid HomeTeamId, string HomeTeamName, Guid AwayTeamId, string AwayTeamName, int RoundNumber);
