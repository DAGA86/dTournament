using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class TeamAndPlayerValidationTests
{
    [Fact]
    public void Team_Validate_Throws_When_Name_Is_Missing()
    {
        var team = new Team { ShortName = "ABC", Club = "Club", ResponsiblePerson = "Manager" };
        Assert.Throws<InvalidOperationException>(() => team.Validate());
    }

    [Fact]
    public void Player_Validate_Throws_When_BirthYear_Is_Outside_AgeGroup_Without_Override()
    {
        var ageGroup = new AgeGroup { Name = "Benjamins", BirthYearFrom = 2014, BirthYearTo = 2015 };
        var player = new Player { FullName = "Older Player", DisplayName = "Older", BirthDate = new DateOnly(2012, 1, 1) };
        Assert.Throws<InvalidOperationException>(() => player.Validate(ageGroup));
    }

    [Fact]
    public void Player_Validate_Allows_BirthYear_Outside_AgeGroup_With_Override()
    {
        var ageGroup = new AgeGroup { Name = "Benjamins", BirthYearFrom = 2014, BirthYearTo = 2015 };
        var player = new Player { FullName = "Older Player", DisplayName = "Older", BirthDate = new DateOnly(2012, 1, 1), AgeOverrideApproved = true };
        player.Validate(ageGroup);
    }

    [Fact]
    public void Player_Validate_Throws_When_ShirtNumber_Is_Invalid()
    {
        var ageGroup = new AgeGroup { Name = "Benjamins", BirthYearFrom = 2014, BirthYearTo = 2015 };
        var player = new Player { FullName = "Player", DisplayName = "Player", BirthDate = new DateOnly(2014, 1, 1), ShirtNumber = 0 };
        Assert.Throws<InvalidOperationException>(() => player.Validate(ageGroup));
    }
}
