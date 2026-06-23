using TournamentManager.Domain.Enums;
using Xunit;

namespace TournamentManager.Tests;

public sealed class MatchClockTests
{
    [Fact]
    public void GetElapsedPlayingTime_Excludes_Paused_Seconds()
    {
        var start = DateTimeOffset.UtcNow.AddMinutes(-10);
        var match = new Domain.Entities.Match { ActualStartUtc = start, Status = MatchStatus.InProgress, TotalPausedSeconds = 120 };
        var elapsed = match.GetElapsedPlayingTime(start.AddMinutes(10));
        Assert.Equal(8, (int)elapsed.TotalMinutes);
    }
}