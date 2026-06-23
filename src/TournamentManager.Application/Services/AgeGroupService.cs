using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class AgeGroupService(IAgeGroupRepository ageGroupRepository, ITournamentRepository tournamentRepository)
{
    public async Task<IReadOnlyList<AgeGroupDto>> ListByTournamentAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        var ageGroups = await ageGroupRepository.ListByTournamentAsync(tournamentId, cancellationToken);
        return ageGroups.Select(x => new AgeGroupDto(x.Id, x.TournamentId, x.Name, x.MatchDurationMinutes, x.CompetitionFormat)).ToList();
    }

    public async Task<Guid> CreateAsync(Guid tournamentId, string name, int? birthYearFrom, int? birthYearTo, int matchDurationMinutes, CompetitionFormat competitionFormat, CancellationToken cancellationToken = default)
    {
        _ = await tournamentRepository.GetAsync(tournamentId, cancellationToken) ?? throw new InvalidOperationException("Tournament was not found.");
        var ageGroup = new AgeGroup { TournamentId = tournamentId, Name = name.Trim(), BirthYearFrom = birthYearFrom, BirthYearTo = birthYearTo, MatchDurationMinutes = matchDurationMinutes, CompetitionFormat = competitionFormat };
        ageGroup.Validate();
        await ageGroupRepository.AddAsync(ageGroup, cancellationToken);
        await ageGroupRepository.SaveChangesAsync(cancellationToken);
        return ageGroup.Id;
    }
}
