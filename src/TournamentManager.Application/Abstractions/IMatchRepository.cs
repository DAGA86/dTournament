using System.Text.RegularExpressions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Abstractions;

public interface IMatchRepository
{
    Task<IReadOnlyList<Domain.Entities.Match>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Match>> ListByAgeGroupWithTeamsAsync(Guid ageGroupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Match>> ListFinishedByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Match>> ListFinishedByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Match?> GetForManagementAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<bool> HasMatchesForAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Domain.Entities.Match> matches, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Match match, CancellationToken cancellationToken = default);
    Task AddGoalAsync(GoalEvent goalEvent, CancellationToken cancellationToken = default);
    Task<GoalEvent?> GetGoalAsync(Guid goalEventId, CancellationToken cancellationToken = default);
    Task<PlayerOfTheMatchVote?> GetVoteAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default);
    Task AddVoteAsync(PlayerOfTheMatchVote vote, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}