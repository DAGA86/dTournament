using Microsoft.EntityFrameworkCore;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Infrastructure.Data;

public sealed class MatchRepository(ApplicationDbContext dbContext) : IMatchRepository
{
    public async Task<IReadOnlyList<Match>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => await dbContext.Matches.Include(x => x.HomeTeam).Include(x => x.AwayTeam).Include(x => x.Venue).Where(x => x.AgeGroupId == ageGroupId).OrderBy(x => x.RoundNumber).ThenBy(x => x.ScheduledStartUtc).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Match>> ListByAgeGroupWithTeamsAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => await dbContext.Matches.Include(x => x.HomeTeam).Include(x => x.AwayTeam).Where(x => x.AgeGroupId == ageGroupId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Match>> ListFinishedByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => await dbContext.Matches
        .Include(x => x.GoalEvents).ThenInclude(x => x.Player).ThenInclude(x => x!.Team)
        .Include(x => x.PlayerOfTheMatchVotes).ThenInclude(x => x.Player).ThenInclude(x => x!.Team)
        .Where(x => x.AgeGroupId == ageGroupId && x.Status == TournamentManager.Domain.Enums.MatchStatus.Finished)
        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Match>> ListFinishedByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => await dbContext.Matches
        .Include(x => x.GoalEvents).ThenInclude(x => x.Player).ThenInclude(x => x!.Team)
        .Include(x => x.PlayerOfTheMatchVotes).ThenInclude(x => x.Player).ThenInclude(x => x!.Team)
        .Where(x => x.TournamentId == tournamentId && x.Status == TournamentManager.Domain.Enums.MatchStatus.Finished)
        .ToListAsync(cancellationToken);
    public Task<Match?> GetForManagementAsync(Guid matchId, CancellationToken cancellationToken = default) => dbContext.Matches.Include(x => x.HomeTeam).ThenInclude(x => x!.Players).Include(x => x.AwayTeam).ThenInclude(x => x!.Players).Include(x => x.GoalEvents).ThenInclude(x => x.Team).Include(x => x.GoalEvents).ThenInclude(x => x.Player).Include(x => x.PlayerOfTheMatchVotes).FirstOrDefaultAsync(x => x.Id == matchId, cancellationToken);
    public Task<bool> HasMatchesForAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => dbContext.Matches.AnyAsync(x => x.AgeGroupId == ageGroupId, cancellationToken);
    public async Task<DateTimeOffset?> GetFirstScheduledStartForTeamAsync(Guid teamId, CancellationToken cancellationToken = default) => await dbContext.Matches
        .Where(x => (x.HomeTeamId == teamId || x.AwayTeamId == teamId) && x.ScheduledStartUtc.HasValue)
        .OrderBy(x => x.ScheduledStartUtc)
        .Select(x => x.ScheduledStartUtc)
        .FirstOrDefaultAsync(cancellationToken);
    public async Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default) => await dbContext.Matches.AddRangeAsync(matches, cancellationToken);
    public async Task AddAsync(Match match, CancellationToken cancellationToken = default) => await dbContext.Matches.AddAsync(match, cancellationToken);
    public async Task AddGoalAsync(GoalEvent goalEvent, CancellationToken cancellationToken = default) => await dbContext.GoalEvents.AddAsync(goalEvent, cancellationToken);
    public Task<GoalEvent?> GetGoalAsync(Guid goalEventId, CancellationToken cancellationToken = default) => dbContext.GoalEvents.Include(x => x.Match).ThenInclude(x => x!.GoalEvents).FirstOrDefaultAsync(x => x.Id == goalEventId, cancellationToken);
    public Task<PlayerOfTheMatchVote?> GetVoteAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default) => dbContext.PlayerOfTheMatchVotes.FirstOrDefaultAsync(x => x.MatchId == matchId && x.TeamId == teamId && !x.IsGoalkeeperVote, cancellationToken);
    public Task<PlayerOfTheMatchVote?> GetGoalkeeperVoteAsync(Guid matchId, CancellationToken cancellationToken = default) => dbContext.PlayerOfTheMatchVotes.FirstOrDefaultAsync(x => x.MatchId == matchId && x.IsGoalkeeperVote, cancellationToken);
    public async Task AddVoteAsync(PlayerOfTheMatchVote vote, CancellationToken cancellationToken = default) => await dbContext.PlayerOfTheMatchVotes.AddAsync(vote, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
