using TournamentManager.Domain.Enums;

namespace TournamentManager.Domain.Entities;

public sealed class Match : BaseEntity
{
    public Guid TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public Guid AgeGroupId { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    public CompetitionPhase Phase { get; set; } = CompetitionPhase.League;
    public int RoundNumber { get; set; }
    public Guid? HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }
    public Guid? AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }
    public DateTimeOffset? ScheduledStartUtc { get; set; }
    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public int PlannedDurationMinutes { get; set; } = 20;
    public int PlannedPeriodCount { get; set; } = 1;
    public int HalfTimeBreakMinutes { get; set; }
    public int CurrentPeriodNumber { get; set; } = 1;
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public DateTimeOffset? ActualStartUtc { get; set; }
    public DateTimeOffset? ActualEndUtc { get; set; }
    public DateTimeOffset? PausedAtUtc { get; set; }
    public long TotalPausedSeconds { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public int? HomeRegularTimeScore { get; set; }
    public int? AwayRegularTimeScore { get; set; }
    public int? HomeExtraTimeScore { get; set; }
    public int? AwayExtraTimeScore { get; set; }
    public int? HomePenaltyScore { get; set; }
    public int? AwayPenaltyScore { get; set; }
    public string? Notes { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public ICollection<GoalEvent> GoalEvents { get; set; } = new List<GoalEvent>();
    public ICollection<PlayerOfTheMatchVote> PlayerOfTheMatchVotes { get; set; } = new List<PlayerOfTheMatchVote>();

    public bool IsKnockout => Phase is CompetitionPhase.SemiFinal or CompetitionPhase.ThirdPlace or CompetitionPhase.Final;

    public Guid? GetWinnerTeamId()
    {
        if (!IsKnockout || !HomeTeamId.HasValue || !AwayTeamId.HasValue) return null;
        var homeTotal = HomeScore ?? 0;
        var awayTotal = AwayScore ?? 0;
        if (homeTotal > awayTotal) return HomeTeamId;
        if (awayTotal > homeTotal) return AwayTeamId;
        if (HomePenaltyScore.HasValue && AwayPenaltyScore.HasValue && HomePenaltyScore != AwayPenaltyScore) return HomePenaltyScore > AwayPenaltyScore ? HomeTeamId : AwayTeamId;
        return null;
    }

    public Guid? GetLoserTeamId()
    {
        var winner = GetWinnerTeamId();
        if (!winner.HasValue) return null;
        return winner == HomeTeamId ? AwayTeamId : HomeTeamId;
    }

    public TimeSpan GetElapsedPlayingTime(DateTimeOffset nowUtc)
    {
        if (!ActualStartUtc.HasValue) return TimeSpan.Zero;
        var end = ActualEndUtc ?? (Status == MatchStatus.Paused && PausedAtUtc.HasValue ? PausedAtUtc.Value : nowUtc);
        var elapsedSeconds = Math.Max(0, (long)(end - ActualStartUtc.Value).TotalSeconds - TotalPausedSeconds);
        return TimeSpan.FromSeconds(elapsedSeconds);
    }

    public int PlannedPeriodDurationMinutes => PlannedPeriodCount <= 0 ? PlannedDurationMinutes : (int)Math.Ceiling((double)PlannedDurationMinutes / PlannedPeriodCount);

    public int GetCurrentPeriodNumber(DateTimeOffset nowUtc) => Math.Clamp(CurrentPeriodNumber, 1, Math.Max(PlannedPeriodCount, 1));

    public int GetCurrentMatchMinute(DateTimeOffset nowUtc) => GetCurrentMatchMinute(nowUtc, GetCurrentPeriodNumber(nowUtc));

    public int GetCurrentMatchMinute(DateTimeOffset nowUtc, int periodNumber) => NormalizeMatchMinute((int)Math.Floor(GetElapsedPlayingTime(nowUtc).TotalMinutes), periodNumber);

    public string FormatMatchMinute(int matchMinute, int periodNumber)
    {
        var normalizedPeriod = Math.Clamp(periodNumber, 1, Math.Max(PlannedPeriodCount, 1));
        var periodEndMinute = PlannedPeriodCount <= 1
            ? PlannedDurationMinutes
            : Math.Min(normalizedPeriod * PlannedPeriodDurationMinutes, PlannedDurationMinutes);

        return matchMinute > periodEndMinute ? $"{periodEndMinute}+{matchMinute - periodEndMinute}" : matchMinute.ToString();
    }

    public void NormalizeElapsedPlayingTimeToCurrentPeriodStart(DateTimeOffset nowUtc)
    {
        if (!ActualStartUtc.HasValue || PlannedPeriodCount <= 1) return;

        var normalizedPeriod = Math.Clamp(CurrentPeriodNumber, 1, PlannedPeriodCount);
        var periodStartMinute = (normalizedPeriod - 1) * PlannedPeriodDurationMinutes;
        var targetElapsedSeconds = Math.Min(periodStartMinute, PlannedDurationMinutes) * 60L;
        var totalClockSeconds = Math.Max(0, (long)(nowUtc - ActualStartUtc.Value).TotalSeconds);
        if (totalClockSeconds < targetElapsedSeconds)
        {
            ActualStartUtc = nowUtc.AddSeconds(-targetElapsedSeconds);
            TotalPausedSeconds = 0;
            return;
        }

        TotalPausedSeconds = totalClockSeconds - targetElapsedSeconds;
    }
    
    public int NormalizeMatchMinute(int elapsedMinute, int periodNumber = 1)
    {
        if (elapsedMinute <= 0) return 0;
        if (PlannedPeriodCount <= 1) return elapsedMinute;

        var periodDuration = PlannedPeriodDurationMinutes;
        if (periodDuration <= 0) return elapsedMinute;

        var normalizedPeriod = Math.Clamp(periodNumber, 1, PlannedPeriodCount);
        var periodStartMinute = (normalizedPeriod - 1) * periodDuration;

        return Math.Max(elapsedMinute, periodStartMinute);
    }

    public void Validate()
    {
        if (HomeTeamId.HasValue && AwayTeamId.HasValue && HomeTeamId == AwayTeamId) throw new InvalidOperationException("A team cannot play against itself.");
        if (RoundNumber <= 0) throw new InvalidOperationException("Round number must be greater than zero.");
        if (PlannedDurationMinutes <= 0) throw new InvalidOperationException("Planned duration must be greater than zero.");
        if (PlannedPeriodCount is < 1 or > 2) throw new InvalidOperationException("Planned period count must be one or two.");
        if (HalfTimeBreakMinutes < 0) throw new InvalidOperationException("Half-time break cannot be negative.");
        if (CurrentPeriodNumber < 1 || CurrentPeriodNumber > PlannedPeriodCount) throw new InvalidOperationException("Current period number must be within the planned period range.");
    }
}
