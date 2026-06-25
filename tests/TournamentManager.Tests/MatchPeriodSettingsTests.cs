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
    public void AgeGroup_Validate_Throws_When_Period_Count_Is_Invalid()
    {
        var ageGroup = new AgeGroup { Name = "Infantis", MatchDurationMinutes = 30, NumberOfPeriods = 3 };
        Assert.Throws<InvalidOperationException>(() => ageGroup.Validate());
    }

    [Fact]
    public void Match_Validate_Throws_When_Period_Count_Is_Invalid()
    {
        var match = new Match { TournamentId = Guid.NewGuid(), AgeGroupId = Guid.NewGuid(), HomeTeamId = Guid.NewGuid(), AwayTeamId = Guid.NewGuid(), RoundNumber = 1, PlannedPeriodCount = 3 };
        Assert.Throws<InvalidOperationException>(() => match.Validate());
    }

    [Fact]
    public void Match_NormalizeMatchMinute_Allows_One_Period_Overtime()
    {
        var match = new Match { PlannedDurationMinutes = 30, PlannedPeriodCount = 1 };
        Assert.Equal(32, match.NormalizeMatchMinute(32));
    }

    [Fact]
    public void Match_NormalizeMatchMinute_Allows_Overtime_In_Selected_Period()
    {
        var match = new Match { PlannedDurationMinutes = 30, PlannedPeriodCount = 2 };
        Assert.Equal(16, match.NormalizeMatchMinute(16, periodNumber: 1));
        Assert.Equal(32, match.NormalizeMatchMinute(32, periodNumber: 2));
    }

    [Fact]
    public void Match_FormatMatchMinute_Uses_Added_Time_Only_After_Current_Period_End()
    {
        var match = new Match { PlannedDurationMinutes = 30, PlannedPeriodCount = 2 };

        Assert.Equal("15+1", match.FormatMatchMinute(16, periodNumber: 1));
        Assert.Equal("16", match.FormatMatchMinute(16, periodNumber: 2));
    }

    [Fact]
    public void Match_NormalizeElapsedPlayingTimeToCurrentPeriodStart_Starts_Next_Period_At_Planned_Minute()
    {
        var start = DateTimeOffset.UtcNow;
        var match = new Match { ActualStartUtc = start, PlannedDurationMinutes = 30, PlannedPeriodCount = 2, CurrentPeriodNumber = 2 };

        match.NormalizeElapsedPlayingTimeToCurrentPeriodStart(start.AddMinutes(16).AddSeconds(20));

        Assert.Equal(TimeSpan.FromMinutes(15), match.GetElapsedPlayingTime(start.AddMinutes(16).AddSeconds(20)));
    }

    [Fact]
    public void Match_GetCurrentPeriodNumber_Uses_Persisted_Period_Not_Elapsed_Playing_Time()
    {
        var start = DateTimeOffset.UtcNow.AddMinutes(-16);
        var match = new Match { ActualStartUtc = start, PlannedDurationMinutes = 30, PlannedPeriodCount = 2, CurrentPeriodNumber = 1 };

        Assert.Equal(1, match.GetCurrentPeriodNumber(start.AddMinutes(16)));
    }

    [Fact]
    public void Match_GetCurrentMatchMinute_Uses_Current_Persisted_Period_Automatically()
    {
        var start = DateTimeOffset.UtcNow.AddMinutes(-16);
        var match = new Match { ActualStartUtc = start, PlannedDurationMinutes = 30, PlannedPeriodCount = 2, CurrentPeriodNumber = 1 };

        Assert.Equal(16, match.GetCurrentMatchMinute(start.AddMinutes(16)));
    }
}
