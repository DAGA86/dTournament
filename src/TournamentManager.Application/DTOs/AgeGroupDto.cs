using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record AgeGroupDto(Guid Id, Guid TournamentId, string Name, int MatchDurationMinutes, CompetitionFormat CompetitionFormat);