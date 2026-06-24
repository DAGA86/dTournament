using TournamentManager.Application.Abstractions;
using TournamentManager.Application.Services;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;
using Xunit;

namespace TournamentManager.Tests;

public sealed class MatchManagementServiceTests
{
    [Fact]
    public async Task GetControlAsync_Orders_Goals_By_Period_Then_Minute()
    {
        var home = NewTeam("Home");
        var away = NewTeam("Away");
        var match = NewMatch(home, away);
        var firstPeriodAddedTime = NewGoal(match, home, NewPlayer(home, "First period"), 16, 1, DateTimeOffset.UtcNow.AddSeconds(1));
        var secondPeriodRegularTime = NewGoal(match, away, NewPlayer(away, "Second period"), 15, 2, DateTimeOffset.UtcNow);
        match.GoalEvents.Add(secondPeriodRegularTime);
        match.GoalEvents.Add(firstPeriodAddedTime);
        var service = new MatchManagementService(new FakeMatchRepository(match), new FakePlayerRepository());

        var control = await service.GetControlAsync(match.Id);

        Assert.Equal(new[] { firstPeriodAddedTime.Id, secondPeriodRegularTime.Id }, control.Goals.Select(x => x.Id));
    }

    [Fact]
    public async Task RegisterGoalAsync_Marks_Own_Goal_When_Player_Belongs_To_Opponent()
    {
        var home = NewTeam("Home");
        var away = NewTeam("Away");
        var awayPlayer = NewPlayer(away, "Away player");
        var match = NewMatch(home, away);
        var matchRepository = new FakeMatchRepository(match);
        var service = new MatchManagementService(matchRepository, new FakePlayerRepository(awayPlayer));

        await service.RegisterGoalAsync(match.Id, home.Id, awayPlayer.Id, "operator");

        var goal = Assert.Single(matchRepository.AddedGoals);
        Assert.True(goal.IsOwnGoal);
        Assert.Equal(home.Id, goal.TeamId);
        Assert.Equal(awayPlayer.Id, goal.PlayerId);
    }

    [Fact]
    public async Task RegisterGoalAsync_Does_Not_Mark_Own_Goal_When_Player_Belongs_To_Scoring_Team()
    {
        var home = NewTeam("Home");
        var away = NewTeam("Away");
        var homePlayer = NewPlayer(home, "Home player");
        var match = NewMatch(home, away);
        var matchRepository = new FakeMatchRepository(match);
        var service = new MatchManagementService(matchRepository, new FakePlayerRepository(homePlayer));

        await service.RegisterGoalAsync(match.Id, home.Id, homePlayer.Id, "operator");

        Assert.False(Assert.Single(matchRepository.AddedGoals).IsOwnGoal);
    }

    private static Match NewMatch(Team home, Team away) => new()
    {
        Id = Guid.NewGuid(),
        TournamentId = Guid.NewGuid(),
        AgeGroupId = Guid.NewGuid(),
        HomeTeamId = home.Id,
        HomeTeam = home,
        AwayTeamId = away.Id,
        AwayTeam = away,
        RoundNumber = 1,
        Status = MatchStatus.InProgress,
        ActualStartUtc = DateTimeOffset.UtcNow.AddMinutes(-16),
        PlannedDurationMinutes = 30,
        PlannedPeriodCount = 2,
        CurrentPeriodNumber = 1
    };

    private static Team NewTeam(string name) => new() { Id = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), Name = name, ShortName = name[..1], Club = "Club" };

    private static Player NewPlayer(Team team, string name)
    {
        var player = new Player { Id = Guid.NewGuid(), TeamId = team.Id, Team = team, FullName = name, DisplayName = name, BirthDate = new DateOnly(2015, 1, 1), IsActive = true };
        team.Players.Add(player);
        return player;
    }

    private static GoalEvent NewGoal(Match match, Team team, Player player, int minute, int period, DateTimeOffset recordedAt) => new()
    {
        Id = Guid.NewGuid(),
        MatchId = match.Id,
        Match = match,
        TeamId = team.Id,
        Team = team,
        PlayerId = player.Id,
        Player = player,
        MatchMinute = minute,
        MatchPeriodNumber = period,
        RecordedAtUtc = recordedAt,
        RecordedByUserId = "operator"
    };

    private sealed class FakeMatchRepository(params Match[] matches) : IMatchRepository
    {
        public List<GoalEvent> AddedGoals { get; } = [];
        public Task<IReadOnlyList<Match>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<IReadOnlyList<Match>> ListByAgeGroupWithTeamsAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<IReadOnlyList<Match>> ListFinishedByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<IReadOnlyList<Match>> ListFinishedByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches);
        public Task<Match?> GetForManagementAsync(Guid matchId, CancellationToken cancellationToken = default) => Task.FromResult(matches.FirstOrDefault(x => x.Id == matchId));
        public Task<bool> HasMatchesForAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult(matches.Any());
        public Task<DateTimeOffset?> GetFirstScheduledStartForTeamAsync(Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<DateTimeOffset?>(matches.Where(x => (x.HomeTeamId == teamId || x.AwayTeamId == teamId) && x.ScheduledStartUtc.HasValue).OrderBy(x => x.ScheduledStartUtc).Select(x => x.ScheduledStartUtc).FirstOrDefault());
        public Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddAsync(Match match, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddGoalAsync(GoalEvent goalEvent, CancellationToken cancellationToken = default) { AddedGoals.Add(goalEvent); return Task.CompletedTask; }
        public Task<GoalEvent?> GetGoalAsync(Guid goalEventId, CancellationToken cancellationToken = default) => Task.FromResult<GoalEvent?>(null);
        public Task<PlayerOfTheMatchVote?> GetVoteAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<PlayerOfTheMatchVote?>(null);
        public Task AddVoteAsync(PlayerOfTheMatchVote vote, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePlayerRepository(params Player[] players) : IPlayerRepository
    {
        public Task<IReadOnlyList<Player>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Player>>(players.Where(x => x.TeamId == teamId).ToList());
        public Task<Player?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(players.FirstOrDefault(x => x.Id == id));
        public Task<bool> ShirtNumberExistsAsync(Guid teamId, int shirtNumber, Guid? excludingPlayerId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Player player, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ReplaceForTeamAsync(Guid teamId, IReadOnlyList<Player> players, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
