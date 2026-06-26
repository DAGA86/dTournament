using TournamentManager.Application.Abstractions;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class KnockoutProgressionService(IAgeGroupRepository ageGroupRepository, IMatchRepository matchRepository)
{
    public async Task<IReadOnlyList<Domain.Entities.Match>> ListFinalsAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.ListByAgeGroupWithTeamsAsync(ageGroupId, cancellationToken);
        return matches.Where(x => x.Phase is CompetitionPhase.RoundOf16 or CompetitionPhase.QuarterFinal or CompetitionPhase.SemiFinal or CompetitionPhase.Final or CompetitionPhase.ThirdPlace).OrderBy(x => x.Phase).ToList();
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
}
