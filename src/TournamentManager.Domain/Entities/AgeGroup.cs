using TournamentManager.Domain.Enums;

namespace TournamentManager.Domain.Entities;

public sealed class AgeGroup : BaseEntity
{
    public const int NameMaxLength = 120;
    public Guid TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int? BirthYearFrom { get; set; }
    public int? BirthYearTo { get; set; }
    public int MatchDurationMinutes { get; set; } = 20;
    public int NumberOfPeriods { get; set; } = 1;
    public int HalfTimeBreakMinutes { get; set; }
    public int PointsPerWin { get; set; } = 3;
    public int PointsPerDraw { get; set; } = 1;
    public int PointsPerLoss { get; set; }
    public CompetitionFormat CompetitionFormat { get; set; } = CompetitionFormat.RoundRobin;
    public int RoundRobinLegs { get; set; } = 1;
    public int? FixedMatchesPerTeam { get; set; }
    public int GroupCount { get; set; } = 1;
    public int AdvancingTeamsPerGroup { get; set; } = 2;
    public CompetitionPhase FinalsStartPhase { get; set; } = CompetitionPhase.SemiFinal;
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<KnockoutAssignment> KnockoutAssignments { get; set; } = new List<KnockoutAssignment>();
    public string TieBreakerOrder { get; set; } = string.Join(',', TieBreaker.Points, TieBreaker.HeadToHead, TieBreaker.GoalDifference, TieBreaker.GoalsFor, TieBreaker.GoalsAgainst, TieBreaker.ManualDecision);

    public void Validate()
    {
        if (MatchDurationMinutes <= 0) throw new InvalidOperationException("Match duration must be greater than zero.");
        if (NumberOfPeriods is < 1 or > 2) throw new InvalidOperationException("Number of periods must be one or two.");
        if (HalfTimeBreakMinutes < 0) throw new InvalidOperationException("Half-time break cannot be negative.");
        if (BirthYearFrom.HasValue && BirthYearTo.HasValue && BirthYearFrom > BirthYearTo) throw new InvalidOperationException("The first birth year cannot be greater than the last birth year.");
        if (RoundRobinLegs is < 1 or > 2) throw new InvalidOperationException("Round robin legs must be one or two.");
        if (CompetitionFormat == CompetitionFormat.GroupStageAndFinals && GroupCount < 1) throw new InvalidOperationException("At least one group is required.");
        if (CompetitionFormat == CompetitionFormat.GroupStageAndFinals && AdvancingTeamsPerGroup < 1) throw new InvalidOperationException("At least one team per group must qualify.");
    }
}
