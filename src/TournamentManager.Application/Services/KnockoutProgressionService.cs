using System.Text.RegularExpressions;
using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class KnockoutProgressionService(IAgeGroupRepository ageGroupRepository, IMatchRepository matchRepository)
{
    public async Task ProgressNextRoundAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        var matches = (await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken)).ToList();
        var currentPhase = new[] { CompetitionPhase.SemiFinal, CompetitionPhase.QuarterFinal, CompetitionPhase.RoundOf16 }.FirstOrDefault(phase => matches.Any(x => x.Phase == phase) && !matches.Any(x => x.Phase == NextPhase(phase)));
        if (currentPhase == default) throw new InvalidOperationException("There is no knockout phase ready to progress.");
        var currentMatches = matches.Where(x => x.Phase == currentPhase).OrderBy(x => x.RoundNumber).ToList();
        if (currentMatches.Any(x => x.Status != MatchStatus.Finished)) throw new InvalidOperationException("All matches in the current phase must be finished before creating the next phase.");
        var winners = currentMatches.Select(x => x.GetWinnerTeamId()).ToList();
        if (winners.Any(x => !x.HasValue)) throw new InvalidOperationException("Knockout matches must have a definitive winner.");
        if (currentPhase == CompetitionPhase.SemiFinal)
        {
            var losers = currentMatches.Select(x => x.GetLoserTeamId()).ToList();
            if (losers.Any(x => !x.HasValue)) throw new InvalidOperationException("Knockout matches must have a definitive winner.");
            await UpsertKnockoutMatchAsync(ageGroup, matches, CompetitionPhase.Final, 1, winners[0]!.Value, winners[1]!.Value, cancellationToken);
            await UpsertKnockoutMatchAsync(ageGroup, matches, CompetitionPhase.ThirdPlace, 1, losers[0]!.Value, losers[1]!.Value, cancellationToken);
        }
        else
        {
            var nextPhase = NextPhase(currentPhase);
            for (var index = 0; index < winners.Count; index += 2) await UpsertKnockoutMatchAsync(ageGroup, matches, nextPhase, index / 2 + 1, winners[index]!.Value, winners[index + 1]!.Value, cancellationToken);
        }
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    private static CompetitionPhase NextPhase(CompetitionPhase phase) => phase switch
    {
        CompetitionPhase.RoundOf16 => CompetitionPhase.QuarterFinal,
        CompetitionPhase.QuarterFinal => CompetitionPhase.SemiFinal,
        CompetitionPhase.SemiFinal => CompetitionPhase.Final,
        _ => CompetitionPhase.Final
    }; 
    
    public async Task<IReadOnlyList<Domain.Entities.Match>> ListFinalsAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken);
        return matches.Where(x => x.Phase is CompetitionPhase.RoundOf16 or CompetitionPhase.QuarterFinal or CompetitionPhase.SemiFinal or CompetitionPhase.Final or CompetitionPhase.ThirdPlace).OrderBy(x => x.Phase).ThenBy(x => x.RoundNumber).ToList();
    }


    public async Task<IReadOnlyList<TournamentManager.Application.DTOs.StandingDto>> CalculateFinalTopFourAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken);
        var final = matches.FirstOrDefault(x => x.Phase == CompetitionPhase.Final && x.Status == MatchStatus.Finished);
        if (final is null) return Array.Empty<TournamentManager.Application.DTOs.StandingDto>();
        var rows = new List<TournamentManager.Application.DTOs.StandingDto>();
        var winnerId = final.GetWinnerTeamId();
        var loserId = final.GetLoserTeamId();
        if (winnerId.HasValue) rows.Add(new(winnerId.Value, winnerId == final.HomeTeamId ? final.HomeTeam?.Name ?? string.Empty : final.AwayTeam?.Name ?? string.Empty, 1, 0, 0, 0, 0, 0, 0, 0, 0));
        if (loserId.HasValue) rows.Add(new(loserId.Value, loserId == final.HomeTeamId ? final.HomeTeam?.Name ?? string.Empty : final.AwayTeam?.Name ?? string.Empty, 2, 0, 0, 0, 0, 0, 0, 0, 0));
        var third = matches.FirstOrDefault(x => x.Phase == CompetitionPhase.ThirdPlace && x.Status == MatchStatus.Finished);
        if (third is not null)
        {
            var thirdId = third.GetWinnerTeamId();
            var fourthId = third.GetLoserTeamId();
            if (thirdId.HasValue) rows.Add(new(thirdId.Value, thirdId == third.HomeTeamId ? third.HomeTeam?.Name ?? string.Empty : third.AwayTeam?.Name ?? string.Empty, 3, 0, 0, 0, 0, 0, 0, 0, 0));
            if (fourthId.HasValue) rows.Add(new(fourthId.Value, fourthId == third.HomeTeamId ? third.HomeTeam?.Name ?? string.Empty : third.AwayTeam?.Name ?? string.Empty, 4, 0, 0, 0, 0, 0, 0, 0, 0));
        }
        return rows;
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
