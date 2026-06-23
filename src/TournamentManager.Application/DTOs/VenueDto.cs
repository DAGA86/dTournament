namespace TournamentManager.Application.DTOs;

public sealed record VenueDto(Guid Id, string Name, string? Description, bool IsActive);
