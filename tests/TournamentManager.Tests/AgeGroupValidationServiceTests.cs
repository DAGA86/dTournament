using TournamentManager.Application.Services;
using TournamentManager.Domain.Entities;
using Xunit;

namespace TournamentManager.Tests;

public sealed class AgeGroupValidationServiceTests
{
    [Fact]
    public void Validate_Throws_When_EndBirthYearIsBeforeStartBirthYear()
    {
        var service = new AgeGroupValidationService();
        var ageGroup = new AgeGroup { Name = "Infantis", BirthYearFrom = 2014, BirthYearTo = 2012 };
        Assert.Throws<InvalidOperationException>(() => service.Validate(ageGroup));
    }

    [Fact]
    public void Validate_Allows_Default_PhaseOne_Configuration()
    {
        var service = new AgeGroupValidationService();
        var ageGroup = new AgeGroup { Name = "Benjamins", BirthYearFrom = 2013, BirthYearTo = 2014 };
        service.Validate(ageGroup);
    }
}