using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;

namespace TournamentManager.Application.Services;

public sealed class PlayerStatisticsService(IMatchRepository matchRepository, ITeamRepository teamRepository)
{
    public async Task<IReadOnlyList<PlayerStatisticDto>> GetTopPlayersByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListFinishedByAgeGroupAsync(ageGroupId, cancellationToken);
        var goals = matches.SelectMany(x => x.GoalEvents).Where(x => x.IsActive && !x.IsOwnGoal).GroupBy(x => x.PlayerId).ToDictionary(x => x.Key, x => x.Count());
        var votes = matches.SelectMany(x => x.PlayerOfTheMatchVotes).Where(x => !x.IsGoalkeeperVote).GroupBy(x => x.PlayerId).ToDictionary(x => x.Key, x => x.Count());
        var goalkeeperVotes = matches.SelectMany(x => x.PlayerOfTheMatchVotes).Where(x => x.IsGoalkeeperVote).GroupBy(x => x.PlayerId).ToDictionary(x => x.Key, x => x.Count());
        return matches.SelectMany(x => x.GoalEvents.Select(g => g.Player).Concat(x.PlayerOfTheMatchVotes.Select(v => v.Player)))
            .Where(x => x is not null)
            .DistinctBy(x => x!.Id)
            .Select(x => new PlayerStatisticDto(x!.Id, x.DisplayName, x.Team?.Name ?? string.Empty, x.ShirtNumber, goals.GetValueOrDefault(x.Id), votes.GetValueOrDefault(x.Id), goalkeeperVotes.GetValueOrDefault(x.Id)))
            .OrderByDescending(x => x.Goals)
            .ThenByDescending(x => x.PlayerOfTheMatchVotes)
            .ThenByDescending(x => x.GoalkeeperOfTheMatchVotes)
            .ThenBy(x => x.PlayerName)
            .ToList();
    }

    public async Task<IReadOnlyList<TeamStatisticDto>> GetTeamStatisticsByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var teams = await teamRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        var matches = await matchRepository.ListFinishedByAgeGroupAsync(ageGroupId, cancellationToken);
        return teams.Select(team =>
        {
            var teamMatches = matches.Where(x => x.HomeTeamId == team.Id || x.AwayTeamId == team.Id).ToList();
            var goalsFor = teamMatches.Sum(x => x.GoalEvents.Count(g => g.IsActive && g.TeamId == team.Id));
            var goalsAgainst = teamMatches.Sum(x => x.GoalEvents.Count(g => g.IsActive && g.TeamId != team.Id));
            return new TeamStatisticDto(team.Id, team.Name, goalsFor, goalsAgainst, team.Players.Count, teamMatches.Count);
        }).OrderBy(x => x.TeamName).ToList();
    }
}
