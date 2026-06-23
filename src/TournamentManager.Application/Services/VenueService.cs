using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class VenueService(IVenueRepository venueRepository)
{
    public async Task<IReadOnlyList<VenueDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var venues = await venueRepository.ListAsync(cancellationToken);
        return venues.Select(v => new VenueDto(v.Id, v.Name, v.Description, v.IsActive)).ToList();
    }

    public async Task<Guid> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        if (await venueRepository.ExistsByNameAsync(name.Trim(), null, cancellationToken)) throw new InvalidOperationException("A venue with the same name already exists.");
        var venue = new Venue { Name = name.Trim(), Description = description };
        venue.Validate();
        await venueRepository.AddAsync(venue, cancellationToken);
        await venueRepository.SaveChangesAsync(cancellationToken);
        return venue.Id;
    }

    public async Task SetActiveAsync(Guid venueId, bool isActive, CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetAsync(venueId, cancellationToken) ?? throw new InvalidOperationException("Venue was not found.");
        venue.IsActive = isActive;
        venue.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await venueRepository.SaveChangesAsync(cancellationToken);
    }
}