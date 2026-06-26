using TournamentManager.Application.DTOs;

namespace TournamentManager.Web.ViewModels;

public sealed record PublicTournamentIndexViewModel(Guid TournamentId, IReadOnlyList<AgeGroupDto> AgeGroups);

public sealed record PublicAgeGroupViewModel(Guid TournamentId, AgeGroupDto AgeGroup);

public sealed record PublicTeamDetailViewModel(TeamDto Team, IReadOnlyList<PlayerGoalSummaryViewModel> Players, IReadOnlyList<TeamStaffMemberDto> StaffMembers, IReadOnlyList<MatchDto> Matches);

public sealed record PlayerGoalSummaryViewModel(Guid Id, string DisplayName, string FullName, int? ShirtNumber, int Goals);
