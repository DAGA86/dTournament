using System.Text.RegularExpressions;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class QualificationService(IAgeGroupRepository ageGroupRepository, IMatchRepository matchRepository, StandingsCalculationService standingsCalculationService)
{
    public async Task CreateSemiFinalsAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var existing = await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken);
        if (existing.Any(x => x.Phase == CompetitionPhase.SemiFinal)) return;
        var qualified = (await standingsCalculationService.CalculateAsync(ageGroupId, cancellationToken)).Take(4).ToList();
        if (qualified.Count < 4) throw new InvalidOperationException("At least four qualified teams are required to create semi-finals.");
        var firstSemiFinal = CreateKnockoutMatch(ageGroup, CompetitionPhase.SemiFinal, 1, qualified[0].TeamId, qualified[3].TeamId);
        var secondSemiFinal = CreateKnockoutMatch(ageGroup, CompetitionPhase.SemiFinal, 2, qualified[1].TeamId, qualified[2].TeamId);
        await matchRepository.AddRangeAsync(new[] { firstSemiFinal, secondSemiFinal }, cancellationToken);
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    private static Domain.Entities.Match CreateKnockoutMatch(AgeGroup ageGroup, CompetitionPhase phase, int roundNumber, Guid homeTeamId, Guid awayTeamId)
    {
        var match = new Domain.Entities.Match { TournamentId = ageGroup.TournamentId, AgeGroupId = ageGroup.Id, Phase = phase, RoundNumber = roundNumber, HomeTeamId = homeTeamId, AwayTeamId = awayTeamId, PlannedDurationMinutes = ageGroup.MatchDurationMinutes };
        match.Validate();
        return match;
    }
}
