using TournamentManager.Application.Abstractions;
using TournamentManager.Application.Services;
using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class MatchScheduleServiceTests
{
    [Fact]
    public async Task ListByAgeGroupAsync_Orders_By_DateTime_Then_Venue_Name()
    {
        var ageGroupId = Guid.NewGuid();
        var early = NewMatch(ageGroupId, new DateTimeOffset(2026, 6, 26, 9, 0, 0, TimeSpan.Zero), "Campo C");
        var sameTimeSecondVenue = NewMatch(ageGroupId, new DateTimeOffset(2026, 6, 26, 10, 0, 0, TimeSpan.Zero), "Campo B");
        var sameTimeFirstVenue = NewMatch(ageGroupId, new DateTimeOffset(2026, 6, 26, 10, 0, 0, TimeSpan.Zero), "Campo A");
        var unscheduled = NewMatch(ageGroupId, null, "Campo A");
        var service = new MatchScheduleService(new FakeAgeGroupRepository(), new FakeTeamRepository(), new FakeMatchRepository(sameTimeSecondVenue, unscheduled, early, sameTimeFirstVenue));

        var matches = await service.ListByAgeGroupAsync(ageGroupId);

        Assert.Equal(new[] { early.Id, sameTimeFirstVenue.Id, sameTimeSecondVenue.Id, unscheduled.Id }, matches.Select(x => x.Id));
    }

    private static Match NewMatch(Guid ageGroupId, DateTimeOffset? scheduledStartUtc, string venueName) => new()
    {
        Id = Guid.NewGuid(),
        TournamentId = Guid.NewGuid(),
        AgeGroupId = ageGroupId,
        HomeTeam = NewTeam("Home"),
        AwayTeam = NewTeam("Away"),
        ScheduledStartUtc = scheduledStartUtc,
        Venue = new Venue { Id = Guid.NewGuid(), Name = venueName }
    };

    private static Team NewTeam(string name) => new() { Id = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), Name = name, ShortName = name[..1], Club = "Club" };

    private sealed class FakeMatchRepository(params Match[] matches) : IMatchRepository
    {
        public Task<IReadOnlyList<Match>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>(matches.Where(x => x.AgeGroupId == ageGroupId).ToList());
        public Task<IReadOnlyList<Match>> ListByAgeGroupWithTeamsAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>([]);
        public Task<IReadOnlyList<Match>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>([]);
        public Task<IReadOnlyList<Match>> ListByTeamWithGoalEventsAsync(Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>([]);
        public Task<IReadOnlyList<Match>> ListFinishedByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>([]);
        public Task<IReadOnlyList<Match>> ListFinishedByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Match>>([]);
        public Task<Match?> GetForManagementAsync(Guid matchId, CancellationToken cancellationToken = default) => Task.FromResult<Match?>(null);
        public Task<bool> HasMatchesForAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<DateTimeOffset?> GetFirstScheduledStartForTeamAsync(Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<DateTimeOffset?>(null);
        public Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddAsync(Match match, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddGoalAsync(GoalEvent goalEvent, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<GoalEvent?> GetGoalAsync(Guid goalEventId, CancellationToken cancellationToken = default) => Task.FromResult<GoalEvent?>(null);
        public Task<PlayerOfTheMatchVote?> GetVoteAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken = default) => Task.FromResult<PlayerOfTheMatchVote?>(null);
        public Task<PlayerOfTheMatchVote?> GetGoalkeeperVoteAsync(Guid matchId, CancellationToken cancellationToken = default) => Task.FromResult<PlayerOfTheMatchVote?>(null);
        public Task AddVoteAsync(PlayerOfTheMatchVote vote, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAgeGroupRepository : IAgeGroupRepository
    {
        public Task<AgeGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<AgeGroup?>(null);
        public Task<IReadOnlyList<AgeGroup>> ListByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AgeGroup>>([]);
        public Task AddAsync(AgeGroup ageGroup, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeTeamRepository : ITeamRepository
    {
        public Task<IReadOnlyList<Team>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Team>>([]);
        public Task<Team?> GetAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Team?>(null);
        public Task<bool> ExistsInAgeGroupAsync(Guid ageGroupId, string name, Guid? excludingTeamId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Team team, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
