using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;
using Xunit;

namespace TournamentManager.Tests;

public sealed class KnockoutProgressionTests
{
    [Fact]
    public void GetWinnerTeamId_Uses_Penalties_When_Knockout_Total_Is_Tied()
    {
        var homeTeamId = Guid.NewGuid();
        var awayTeamId = Guid.NewGuid();
        var match = new Match { TournamentId = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), HomeTeamId = homeTeamId, AwayTeamId = awayTeamId, RoundNumber = 1, Phase = CompetitionPhase.SemiFinal, HomeScore = 1, AwayScore = 1, HomePenaltyScore = 4, AwayPenaltyScore = 3 };
        Assert.Equal(homeTeamId, match.GetWinnerTeamId());
        Assert.Equal(awayTeamId, match.GetLoserTeamId());
    }

    [Fact]
    public void GetWinnerTeamId_Returns_Null_When_Knockout_Has_No_Definitive_Winner()
    {
        var match = new Match { TournamentId = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), HomeTeamId = Guid.NewGuid(), AwayTeamId = Guid.NewGuid(), RoundNumber = 1, Phase = CompetitionPhase.Final, HomeScore = 2, AwayScore = 2 };
        Assert.Null(match.GetWinnerTeamId());
    }
}
