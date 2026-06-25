using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class TournamentService(ITournamentRepository repository)
{
    public async Task<TournamentDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tournament = await repository.GetAsync(id, cancellationToken);
        return tournament is null ? null : new TournamentDto(tournament.Id, tournament.Name, tournament.Location, tournament.StartsOn, tournament.EndsOn, tournament.Status, tournament.AgeGroups.Count);
    }

    public async Task<IReadOnlyList<TournamentDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tournaments = await repository.ListAsync(cancellationToken);
        return tournaments.Select(t => new TournamentDto(t.Id, t.Name, t.Location, t.StartsOn, t.EndsOn, t.Status, t.AgeGroups.Count)).ToList();
    }

    public async Task<Guid> CreateAsync(string name, string? description, string location, DateOnly startsOn, DateOnly endsOn, string? rulesNotes, CancellationToken cancellationToken = default)
    {
        var tournament = new Tournament { Name = name.Trim(), Description = description, Location = location.Trim(), StartsOn = startsOn, EndsOn = endsOn, RulesNotes = rulesNotes };
        tournament.ValidateDates();
        await repository.AddAsync(tournament, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return tournament.Id;
    }
}
