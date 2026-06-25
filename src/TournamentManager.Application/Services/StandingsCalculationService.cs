using System.Text.RegularExpressions;
using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class StandingsCalculationService(IAgeGroupRepository ageGroupRepository, ITeamRepository teamRepository, IMatchRepository matchRepository)
{
    public async Task<IReadOnlyList<StandingDto>> CalculateAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var teams = (await teamRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken)).Where(x => x.IsActive).ToList();
        var matches = await matchRepository.ListFinishedByAgeGroupAsync(ageGroupId, cancellationToken);
        var rows = teams.ToDictionary(x => x.Id, x => new MutableStanding(x.Id, x.Name));

        foreach (var match in matches.Where(x => x.Status == MatchStatus.Finished))
        {
            if (!rows.ContainsKey(match.HomeTeamId) || !rows.ContainsKey(match.AwayTeamId)) continue;
            var homeGoals = CountGoals(match, match.HomeTeamId);
            var awayGoals = CountGoals(match, match.AwayTeamId);
            ApplyMatch(rows[match.HomeTeamId], homeGoals, awayGoals, ageGroup);
            ApplyMatch(rows[match.AwayTeamId], awayGoals, homeGoals, ageGroup);
        }

        return rows.Values
            .OrderByDescending(x => x.Points)
            .ThenByDescending(x => x.GoalDifference)
            .ThenByDescending(x => x.GoalsFor)
            .ThenBy(x => x.GoalsAgainst)
            .ThenBy(x => x.TeamName)
            .Select((x, index) => x.ToDto(index + 1))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<StandingDto>>> CalculateByGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var teams = (await teamRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken)).Where(x => x.IsActive).ToList();
        var matches = (await matchRepository.ListFinishedByAgeGroupAsync(ageGroupId, cancellationToken)).Where(x => x.Phase == CompetitionPhase.GroupStage).ToList();
        var result = new Dictionary<string, IReadOnlyList<StandingDto>>();
        foreach (var group in ageGroup.Groups.OrderBy(x => x.DisplayOrder))
        {
            var groupTeams = teams.Where(x => x.GroupId == group.Id).ToList();
            var rows = groupTeams.ToDictionary(x => x.Id, x => new MutableStanding(x.Id, x.Name));
            foreach (var match in matches.Where(x => x.GroupId == group.Id))
            {
                if (!rows.ContainsKey(match.HomeTeamId) || !rows.ContainsKey(match.AwayTeamId)) continue;
                var homeGoals = CountGoals(match, match.HomeTeamId);
                var awayGoals = CountGoals(match, match.AwayTeamId);
                ApplyMatch(rows[match.HomeTeamId], homeGoals, awayGoals, ageGroup);
                ApplyMatch(rows[match.AwayTeamId], awayGoals, homeGoals, ageGroup);
            }
            result[group.Name] = rows.Values.OrderByDescending(x => x.Points).ThenByDescending(x => x.GoalDifference).ThenByDescending(x => x.GoalsFor).ThenBy(x => x.GoalsAgainst).ThenBy(x => x.TeamName).Select((x, index) => x.ToDto(index + 1)).ToList();
        }
        return result;
    }

    private static int CountGoals(Domain.Entities.Match match, Guid teamId) => match.GoalEvents.Count(x => x.IsActive && x.TeamId == teamId);

    private static void ApplyMatch(MutableStanding standing, int goalsFor, int goalsAgainst, AgeGroup ageGroup)
    {
        standing.Played++;
        standing.GoalsFor += goalsFor;
        standing.GoalsAgainst += goalsAgainst;
        if (goalsFor > goalsAgainst) { standing.Wins++; standing.Points += ageGroup.PointsPerWin; }
        else if (goalsFor == goalsAgainst) { standing.Draws++; standing.Points += ageGroup.PointsPerDraw; }
        else { standing.Losses++; standing.Points += ageGroup.PointsPerLoss; }
    }

    private sealed class MutableStanding(Guid teamId, string teamName)
    {
        public Guid TeamId { get; } = teamId;
        public string TeamName { get; } = teamName;
        public int Played { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points { get; set; }
        public StandingDto ToDto(int position) => new(TeamId, TeamName, position, Played, Wins, Draws, Losses, GoalsFor, GoalsAgainst, GoalDifference, Points);
    }
}
