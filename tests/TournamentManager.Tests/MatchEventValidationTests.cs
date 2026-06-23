using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class MatchEventValidationTests
{
    [Fact]
    public void GoalEvent_Validate_Throws_When_Minute_Is_Negative()
    {
        var goal = new GoalEvent { MatchId = Guid.NewGuid(), TeamId = Guid.NewGuid(), PlayerId = Guid.NewGuid(), MatchMinute = -1, RecordedByUserId = "user" };
        Assert.Throws<InvalidOperationException>(() => goal.Validate());
    }

    [Fact]
    public void PlayerOfTheMatchVote_Validate_Throws_When_User_Is_Missing()
    {
        var vote = new PlayerOfTheMatchVote { MatchId = Guid.NewGuid(), TeamId = Guid.NewGuid(), PlayerId = Guid.NewGuid() };
        Assert.Throws<InvalidOperationException>(() => vote.Validate());
    }

    [Fact]
    public void Match_Validate_Throws_When_Team_Plays_Against_Itself()
    {
        var teamId = Guid.NewGuid();
        var match = new Match { TournamentId = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), HomeTeamId = teamId, AwayTeamId = teamId, RoundNumber = 1 };
        Assert.Throws<InvalidOperationException>(() => match.Validate());
    }
}
