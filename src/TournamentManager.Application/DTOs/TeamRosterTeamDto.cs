namespace TournamentManager.Application.DTOs;

public sealed record TeamRosterTeamDto(Guid Id, string Name, string AgeGroupName, int? BirthYearFrom, int? BirthYearTo);