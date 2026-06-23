using TournamentManager.Application.Services;
using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class ScheduleGenerationServiceTests
{
    [Fact]
    public void GenerateRoundRobin_WithFourTeamsAndOneLeg_CreatesSixMatches()
    {
        var service = new ScheduleGenerationService();
        var teams = CreateTeams(4);
        var matches = service.GenerateRoundRobin(teams, 1);
        Assert.Equal(6, matches.Count);
        Assert.Equal(3, matches.Select(x => x.RoundNumber).Distinct().Count());
    }

    [Fact]
    public void GenerateRoundRobin_WithFourTeamsAndTwoLegs_CreatesTwelveMatches()
    {
        var service = new ScheduleGenerationService();
        var teams = CreateTeams(4);
        var matches = service.GenerateRoundRobin(teams, 2);
        Assert.Equal(12, matches.Count);
    }

    [Fact]
    public void GenerateFixedMatches_Throws_When_TotalTeamSlotsAreOdd()
    {
        var service = new ScheduleGenerationService();
        var teams = CreateTeams(3);
        Assert.Throws<InvalidOperationException>(() => service.GenerateFixedMatches(teams, 1));
    }

    [Fact]
    public void GenerateFixedMatches_WithFourTeamsAndTwoMatchesEach_CreatesFourMatches()
    {
        var service = new ScheduleGenerationService();
        var teams = CreateTeams(4);
        var matches = service.GenerateFixedMatches(teams, 2);
        Assert.Equal(4, matches.Count);
        foreach (var team in teams)
        {
            Assert.Equal(2, matches.Count(x => x.HomeTeamId == team.Id || x.AwayTeamId == team.Id));
        }
    }

    private static IReadOnlyList<Team> CreateTeams(int count) => Enumerable.Range(1, count).Select(index => new Team { Id = Guid.NewGuid(), Name = $"Team {index}", ShortName = $"T{index}", Club = $"Club {index}" }).ToList();
}
