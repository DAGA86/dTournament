namespace TournamentManager.Application.DTOs;

public sealed record TeamStaffMemberDto(Guid Id, Guid TeamId, string FullName, string Role, bool IsActive);