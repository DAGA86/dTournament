using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.DTOs;

public sealed record TournamentDto(Guid Id, string Name, string Location, DateOnly StartsOn, DateOnly EndsOn, TournamentStatus Status, int AgeGroupCount);