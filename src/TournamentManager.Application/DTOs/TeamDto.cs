namespace TournamentManager.Application.DTOs;

public sealed record TeamDto(Guid Id, Guid AgeGroupId, string AgeGroupName, string Name, string ShortName, string Club, string ResponsiblePerson, string? Contact, bool IsActive, int PlayerCount);
