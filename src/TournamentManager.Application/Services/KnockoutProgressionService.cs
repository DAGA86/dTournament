using System.Text.RegularExpressions;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class KnockoutProgressionService(IAgeGroupRepository ageGroupRepository, IMatchRepository matchRepository)
{
    public async Task ProgressAfterSemiFinalsAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var matches = (await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken)).ToList();
        var semiFinals = matches.Where(x => x.Phase == CompetitionPhase.SemiFinal && x.Status == MatchStatus.Finished).OrderBy(x => x.RoundNumber).ToList();
        if (semiFinals.Count < 2) throw new InvalidOperationException("Both semi-finals must be finished before creating finals.");
        var winners = semiFinals.Select(x => x.GetWinnerTeamId()).ToList();
        var losers = semiFinals.Select(x => x.GetLoserTeamId()).ToList();
        if (winners.Any(x => !x.HasValue) || losers.Any(x => !x.HasValue)) throw new InvalidOperationException("Knockout matches must have a definitive winner.");
        await UpsertKnockoutMatchAsync(ageGroup, matches, CompetitionPhase.Final, 1, winners[0]!.Value, winners[1]!.Value, cancellationToken);
        await UpsertKnockoutMatchAsync(ageGroup, matches, CompetitionPhase.ThirdPlace, 1, losers[0]!.Value, losers[1]!.Value, cancellationToken);
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Match>> ListFinalsAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken);
        return matches.Where(x => x.Phase is CompetitionPhase.SemiFinal or CompetitionPhase.Final or CompetitionPhase.ThirdPlace).OrderBy(x => x.Phase).ThenBy(x => x.RoundNumber).ToList();
    }

    private async Task UpsertKnockoutMatchAsync(AgeGroup ageGroup, IReadOnlyList<Domain.Entities.Match> matches, CompetitionPhase phase, int roundNumber, Guid homeTeamId, Guid awayTeamId, CancellationToken cancellationToken)
    {
        var existing = matches.FirstOrDefault(x => x.Phase == phase && x.RoundNumber == roundNumber);
        if (existing is not null)
        {
            if (existing.Status == MatchStatus.Finished) return;
            existing.HomeTeamId = homeTeamId;
            existing.AwayTeamId = awayTeamId;
            existing.UpdatedAtUtc = DateTimeOffset.UtcNow;
            return;
        }
        var match = new Domain.Entities.Match { TournamentId = ageGroup.TournamentId, AgeGroupId = ageGroup.Id, Phase = phase, RoundNumber = roundNumber, HomeTeamId = homeTeamId, AwayTeamId = awayTeamId, PlannedDurationMinutes = ageGroup.MatchDurationMinutes };
        match.Validate();
        await matchRepository.AddAsync(match, cancellationToken);
    }
}
