using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class MatchScheduleService(IAgeGroupRepository ageGroupRepository, ITeamRepository teamRepository, IMatchRepository matchRepository, ScheduleGenerationService scheduleGenerationService)
{
    public async Task<IReadOnlyList<MatchDto>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        return matches.Select(x => new MatchDto(x.Id, x.AgeGroupId, x.HomeTeamId, x.HomeTeam?.Name ?? string.Empty, x.AwayTeamId, x.AwayTeam?.Name ?? string.Empty, x.RoundNumber, x.ScheduledStartUtc, x.Venue?.Name, x.PlannedDurationMinutes, x.Status, x.HomeScore, x.AwayScore)).ToList();
    }

    public async Task<IReadOnlyList<MatchDto>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByTeamAsync(teamId, cancellationToken);
        return matches.Select(x => new MatchDto(x.Id, x.AgeGroupId, x.HomeTeamId, x.HomeTeam?.Name ?? string.Empty, x.AwayTeamId, x.AwayTeam?.Name ?? string.Empty, x.RoundNumber, x.ScheduledStartUtc, x.Venue?.Name, x.PlannedDurationMinutes, x.Status, x.HomeScore, x.AwayScore)).ToList();
    }

    public async Task<IReadOnlyList<GeneratedMatchDto>> PreviewAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var teams = (await teamRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken)).Where(x => x.IsActive).ToList();
        return ageGroup.CompetitionFormat switch
        {
            CompetitionFormat.RoundRobin => scheduleGenerationService.GenerateRoundRobin(teams, ageGroup.RoundRobinLegs),
            CompetitionFormat.FixedMatches => scheduleGenerationService.GenerateFixedMatches(teams, ageGroup.FixedMatchesPerTeam ?? 0),
            CompetitionFormat.GroupStageAndFinals => scheduleGenerationService.GenerateRoundRobin(teams, 1),
            _ => throw new InvalidOperationException("Unsupported competition format.")
        };
    }

    public async Task GenerateAsync(Guid ageGroupId, DateTimeOffset firstKickoffUtc, int minutesBetweenMatches, Guid? venueId, CancellationToken cancellationToken = default)
    {
        if (await matchRepository.HasMatchesForAgeGroupAsync(ageGroupId, cancellationToken)) throw new InvalidOperationException("This age group already has matches. Delete or edit them manually before generating a new schedule.");
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var generated = await PreviewAsync(ageGroupId, cancellationToken);
        var matches = generated.Select((x, index) =>
        {
            var match = new Match
            {
                TournamentId = ageGroup.TournamentId,
                AgeGroupId = ageGroup.Id,
                Phase = ageGroup.CompetitionFormat == CompetitionFormat.GroupStageAndFinals ? CompetitionPhase.GroupStage : CompetitionPhase.League,
                RoundNumber = x.RoundNumber,
                HomeTeamId = x.HomeTeamId,
                AwayTeamId = x.AwayTeamId,
                ScheduledStartUtc = firstKickoffUtc.AddMinutes(index * minutesBetweenMatches),
                VenueId = venueId,
                PlannedDurationMinutes = ageGroup.MatchDurationMinutes,
                PlannedPeriodCount = ageGroup.NumberOfPeriods,
                HalfTimeBreakMinutes = ageGroup.HalfTimeBreakMinutes
            };
            match.Validate();
            return match;
        }).ToList();
        await matchRepository.AddRangeAsync(matches, cancellationToken);
        await matchRepository.SaveChangesAsync(cancellationToken);
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
        var match = new Match
        {
            TournamentId = ageGroup.TournamentId,
            AgeGroupId = ageGroup.Id,
            Phase = ageGroup.CompetitionFormat == CompetitionFormat.GroupStageAndFinals ? CompetitionPhase.GroupStage : CompetitionPhase.League,
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

}
