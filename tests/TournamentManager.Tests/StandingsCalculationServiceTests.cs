using TournamentManager.Application.Abstractions;
using TournamentManager.Application.Services;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;
using Xunit;

namespace TournamentManager.Tests;

public sealed class StandingsCalculationServiceTests
{
    [Fact]
    public async Task CalculateAsync_Awards_Configured_Win_Draw_And_Loss_Points()
    {
        var ageGroupId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var home = new Team { Id = Guid.NewGuid(), AgeGroupId = ageGroupId, Name = "Home", ShortName = "H", Club = "Club" };
        var away = new Team { Id = Guid.NewGuid(), AgeGroupId = ageGroupId, Name = "Away", ShortName = "A", Club = "Club" };
        var ageGroup = new AgeGroup { Id = ageGroupId, TournamentId = tournamentId, Name = "U10", PointsPerWin = 3, PointsPerDraw = 1, PointsPerLoss = 0 };
        var match = new Match { Id = Guid.NewGuid(), TournamentId = tournamentId, AgeGroupId = ageGroupId, HomeTeamId = home.Id, AwayTeamId = away.Id, RoundNumber = 1, Status = MatchStatus.Finished };
        match.GoalEvents.Add(new GoalEvent { Id = Guid.NewGuid(), MatchId = match.Id, TeamId = home.Id, PlayerId = Guid.NewGuid(), RecordedByUserId = "user" });
        match.GoalEvents.Add(new GoalEvent { Id = Guid.NewGuid(), MatchId = match.Id, TeamId = home.Id, PlayerId = Guid.NewGuid(), RecordedByUserId = "user" });
        match.GoalEvents.Add(new GoalEvent { Id = Guid.NewGuid(), MatchId = match.Id, TeamId = away.Id, PlayerId = Guid.NewGuid(), RecordedByUserId = "user" });
        var service = new StandingsCalculationService(new FakeAgeGroupRepository(ageGroup), new FakeTeamRepository(home, away), new FakeMatchRepository(match));

        var standings = await service.CalculateAsync(ageGroupId);

        Assert.Equal(home.Id, standings[0].TeamId);
        Assert.Equal(3, standings[0].Points);
        Assert.Equal(0, standings[1].Points);
        Assert.Equal(1, standings[0].GoalDifference);
    }

    private sealed class FakeAgeGroupRepository(AgeGroup ageGroup) : IAgeGroupRepository
    {
        public Task<AgeGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<AgeGroup?>(ageGroup);
        public Task<IReadOnlyList<AgeGroup>> ListByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AgeGroup>>(new[] { ageGroup });
        public Task AddAsync(AgeGroup ageGroup, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeTeamRepository(params Team[] teams) : ITeamRepository
    {
        public Task<IReadOnlyList<Team>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Team>>(teams);
        public Task<Team?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(teams.FirstOrDefault(x => x.Id == id));
        public Task<bool> ExistsInAgeGroupAsync(Guid ageGroupId, string name, Guid? excludingTeamId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Team team, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeMatchRepository(params Match[] matches) : IMatchRepository
    {
        public Task<IReadOnlyList<Match>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<IReadOnlyList<Match>> ListByAgeGroupWithTeamsAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<IReadOnlyList<Match>> ListFinishedByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<IReadOnlyList<Match>> ListFinishedByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<Match?> GetForManagementAsync(Guid matchId, CancellationToken cancellationToken = default) => Task.FromResult(matches.FirstOrDefault(x => x.Id == matchId));
        public Task<bool> HasMatchesForAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult(matches.Any());
        public Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddAsync(Match match, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddGoalAsync(GoalEvent goalEvent, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<GoalEvent?> GetGoalAsync(Guid goalEventId, CancellationToken cancellationToken = default) => Task.FromResult<GoalEvent?>(null);
        public Task<PlayerOfTheMatchVote?> GetVoteAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<PlayerOfTheMatchVote?>(null);
        public Task AddVoteAsync(PlayerOfTheMatchVote vote, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
