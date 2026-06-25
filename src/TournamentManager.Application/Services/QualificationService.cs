using System.Text.RegularExpressions;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class QualificationService(IAgeGroupRepository ageGroupRepository, IMatchRepository matchRepository, StandingsCalculationService standingsCalculationService)
{
    public async Task CreateNextPhaseAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var existing = await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken);
        if (existing.Where(x => x.Phase == CompetitionPhase.GroupStage).Any(x => x.Status != MatchStatus.Finished)) throw new InvalidOperationException("All group-stage matches must be finished before creating the next phase.");
        var phase = ageGroup.FinalsStartPhase;
        if (existing.Any(x => x.Phase == phase)) return;
        var grouped = await standingsCalculationService.CalculateByGroupAsync(ageGroupId, cancellationToken);
        var qualified = grouped.Values.SelectMany(x => x.Take(ageGroup.AdvancingTeamsPerGroup)).ToList();
        var required = phase switch { CompetitionPhase.RoundOf16 => 16, CompetitionPhase.QuarterFinal => 8, CompetitionPhase.SemiFinal => 4, CompetitionPhase.Final => 2, _ => 4 };
        if (qualified.Count < required) throw new InvalidOperationException($"At least {required} qualified teams are required to create this phase.");
        qualified = qualified.Take(required).ToList();
        var knockoutMatches = new List<Domain.Entities.Match>();
        for (var index = 0; index < required / 2; index++) knockoutMatches.Add(CreateKnockoutMatch(ageGroup, phase, index + 1, qualified[index].TeamId, qualified[required - index - 1].TeamId));
        await matchRepository.AddRangeAsync(knockoutMatches, cancellationToken);
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    private static Domain.Entities.Match CreateKnockoutMatch(AgeGroup ageGroup, CompetitionPhase phase, int roundNumber, Guid homeTeamId, Guid awayTeamId)
    {
        var match = new Domain.Entities.Match { TournamentId = ageGroup.TournamentId, AgeGroupId = ageGroup.Id, Phase = phase, RoundNumber = roundNumber, HomeTeamId = homeTeamId, AwayTeamId = awayTeamId, PlannedDurationMinutes = ageGroup.MatchDurationMinutes };
        match.Validate();
        return match;
    }
}
