using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class AgeGroupService(IAgeGroupRepository ageGroupRepository, ITournamentRepository tournamentRepository, IMatchRepository matchRepository)
{
    public async Task<IReadOnlyList<AgeGroupDto>> ListByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var ageGroups = await ageGroupRepository.ListByTournamentAsync(tournamentId, cancellationToken);
        return ageGroups.Select(x => new AgeGroupDto(x.Id, x.TournamentId, x.Name, x.MatchDurationMinutes, x.CompetitionFormat)).ToList();
    }

    public async Task<AgeGroup?> GetEntityAsync(Guid ageGroupId, CancellationToken cancellationToken = default) => await ageGroupRepository.GetAsync(ageGroupId, cancellationToken);

    public async Task<Guid> CreateAsync(Guid tournamentId, string name, int? birthYearFrom, int? birthYearTo, int matchDurationMinutes, int numberOfPeriods, int halfTimeBreakMinutes, CompetitionFormat competitionFormat, int groupCount, int advancingTeamsPerGroup, CompetitionPhase finalsStartPhase, IReadOnlyList<PlannedMatchInput>? plannedMatches = null, CancellationToken cancellationToken = default)
    {
        _ = await tournamentRepository.GetAsync(tournamentId, cancellationToken) ?? throw new InvalidOperationException("Tournament was not found.");
        var ageGroup = new AgeGroup
        {
            TournamentId = tournamentId,
            Name = name.Trim(),
            BirthYearFrom = birthYearFrom,
            BirthYearTo = birthYearTo,
            MatchDurationMinutes = matchDurationMinutes,
            NumberOfPeriods = numberOfPeriods,
            HalfTimeBreakMinutes = halfTimeBreakMinutes,
            CompetitionFormat = competitionFormat,
            GroupCount = competitionFormat == CompetitionFormat.GroupStageAndFinals ? groupCount : 1,
            AdvancingTeamsPerGroup = competitionFormat == CompetitionFormat.GroupStageAndFinals ? advancingTeamsPerGroup : 0,
            FinalsStartPhase = finalsStartPhase
        };
        ageGroup.Validate();
        if (ageGroup.CompetitionFormat == CompetitionFormat.GroupStageAndFinals)
        {
            for (var index = 0; index < ageGroup.GroupCount; index++)
            {
                ageGroup.Groups.Add(new Group { AgeGroupId = ageGroup.Id, Name = $"Grupo {(char)('A' + index)}", DisplayOrder = index + 1 });
            }
        }
        await ageGroupRepository.AddAsync(ageGroup, cancellationToken);
        if (plannedMatches is not null)
        {
            foreach (var planned in plannedMatches)
            {
                var match = new Match
                {
                    TournamentId = tournamentId,
                    AgeGroupId = ageGroup.Id,
                    Phase = planned.Phase,
                    RoundNumber = planned.RoundNumber,
                    ScheduledStartUtc = planned.ScheduledStartUtc,
                    VenueId = planned.VenueId,
                    PlannedDurationMinutes = matchDurationMinutes,
                    PlannedPeriodCount = numberOfPeriods,
                    HalfTimeBreakMinutes = halfTimeBreakMinutes
                };
                match.Validate();
                await matchRepository.AddAsync(match, cancellationToken);
            }
        }
        await ageGroupRepository.SaveChangesAsync(cancellationToken);
        return ageGroup.Id;
    }
}

public sealed record PlannedMatchInput(int RoundNumber, CompetitionPhase Phase, DateTimeOffset? ScheduledStartUtc, Guid? VenueId);