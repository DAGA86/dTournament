using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class MatchScheduleService(IAgeGroupRepository ageGroupRepository, ITeamRepository teamRepository, IMatchRepository matchRepository)
{
    public async Task<IReadOnlyList<MatchDto>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        return matches.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<MatchDto>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByTeamAsync(teamId, cancellationToken);
        return matches.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<TournamentManager.Domain.Entities.Match>> ListFinishedGoalEventsByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByTeamWithGoalEventsAsync(teamId, cancellationToken);
        return matches.Where(x => x.Status == MatchStatus.Finished).ToList();
    }
    
    public async Task<Guid> CreateManualAsync(Guid ageGroupId, Guid homeTeamId, Guid awayTeamId, int roundNumber, DateTimeOffset? scheduledStartUtc, Guid? venueId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        if (scheduledStartUtc.HasValue)
        {
            var scheduledDate = DateOnly.FromDateTime(scheduledStartUtc.Value.DateTime);
            if (ageGroup.Tournament is null) throw new InvalidOperationException("Tournament was not found.");
            if (scheduledDate < ageGroup.Tournament.StartsOn || scheduledDate > ageGroup.Tournament.EndsOn)
            {
                throw new InvalidOperationException($"The scheduled date must be between {ageGroup.Tournament.StartsOn:yyyy-MM-dd} and {ageGroup.Tournament.EndsOn:yyyy-MM-dd}.");
            }
        }
        if (homeTeamId == awayTeamId) throw new InvalidOperationException("A team cannot play against itself.");
        var teams = await teamRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        var homeTeam = teams.FirstOrDefault(x => x.Id == homeTeamId) ?? throw new InvalidOperationException("Home team was not found.");
        var awayTeam = teams.FirstOrDefault(x => x.Id == awayTeamId) ?? throw new InvalidOperationException("Away team was not found.");
        if (ageGroup.CompetitionFormat == CompetitionFormat.GroupStageAndFinals && homeTeam.GroupId != awayTeam.GroupId) throw new InvalidOperationException("Group stage matches must be between teams from the same group.");
        var match = new Match
        {
            TournamentId = ageGroup.TournamentId,
            AgeGroupId = ageGroup.Id,
            Phase = ageGroup.CompetitionFormat == CompetitionFormat.GroupStageAndFinals ? CompetitionPhase.GroupStage : CompetitionPhase.League,
            GroupId = ageGroup.CompetitionFormat == CompetitionFormat.GroupStageAndFinals ? homeTeam.GroupId : null,
            RoundNumber = roundNumber,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            ScheduledStartUtc = scheduledStartUtc,
            VenueId = venueId,
            PlannedDurationMinutes = ageGroup.MatchDurationMinutes,
            PlannedPeriodCount = ageGroup.NumberOfPeriods,
            HalfTimeBreakMinutes = ageGroup.HalfTimeBreakMinutes
        };
        match.Validate();
        await matchRepository.AddAsync(match, cancellationToken);
        await matchRepository.SaveChangesAsync(cancellationToken);
        return match.Id;
    }

    private static MatchDto ToDto(Match match)
    {
        var now = DateTimeOffset.UtcNow;
        var periodNumber = match.GetCurrentPeriodNumber(now);
        var currentMinute = match.Status is MatchStatus.InProgress or MatchStatus.Paused ? match.GetCurrentMatchMinute(now, periodNumber) : (int?)null;
        var goals = match.GoalEvents
            .OrderBy(x => x.MatchPeriodNumber)
            .ThenBy(x => x.MatchMinute)
            .ThenBy(x => x.RecordedAtUtc)
            .Select(x => new MatchGoalDto(x.Team?.Name ?? string.Empty, x.Player?.DisplayName ?? string.Empty, x.MatchMinute, x.MatchPeriodNumber, match.FormatMatchMinute(x.MatchMinute, x.MatchPeriodNumber), x.IsOwnGoal, x.IsActive))
            .ToList();

        return new MatchDto(match.Id, match.AgeGroupId, match.HomeTeamId, match.HomeTeam?.Name ?? string.Empty, match.AwayTeamId, match.AwayTeam?.Name ?? string.Empty, match.RoundNumber, match.ScheduledStartUtc, match.Venue?.Name, match.PlannedDurationMinutes, match.Status, match.HomeScore, match.AwayScore, match.Phase, currentMinute, currentMinute.HasValue ? match.FormatMatchMinute(currentMinute.Value, periodNumber) : null, goals);
    }
}
