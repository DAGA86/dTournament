using TournamentManager.Application.DTOs;

namespace TournamentManager.Web.ViewModels.Players;

public sealed record TeamRosterManagementViewModel(Guid TeamId, IReadOnlyList<PlayerDto> Players, IReadOnlyList<TeamStaffMemberDto> StaffMembers);