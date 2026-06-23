using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class MatchPeriodSettingsTests
{
    [Fact]
    public void AgeGroup_Validate_Allows_Two_Periods_With_Break()
    {
        var ageGroup = new AgeGroup { Name = "Infantis", MatchDurationMinutes = 30, NumberOfPeriods = 2, HalfTimeBreakMinutes = 5 };
        ageGroup.Validate();
    }

    [Fact]
    public void Match_Validate_Throws_When_Period_Count_Is_Invalid()
    {
        var match = new Match { TournamentId = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), HomeTeamId = Guid.NewGuid(), AwayTeamId = Guid.NewGuid(), RoundNumber = 1, PlannedPeriodCount = 3 };
        Assert.Throws<InvalidOperationException>(() => match.Validate());
    }
}
