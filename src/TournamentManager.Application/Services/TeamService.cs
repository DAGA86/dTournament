using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class TeamService(ITeamRepository teamRepository, IAgeGroupRepository ageGroupRepository)
{
    public async Task<IReadOnlyList<TeamDto>> ListByAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken = default)
    {
        var teams = await teamRepository.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        return teams.Select(t => new TeamDto(t.Id, t.AgeGroupId, t.AgeGroup?.Name ?? string.Empty, t.Name, t.ShortName, t.Club, t.ResponsiblePerson, t.Contact, t.IsActive, t.Players.Count)).ToList();
    }

    public async Task<TeamDto?> GetAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetAsync(teamId, cancellationToken);
        return team is null ? null : new TeamDto(team.Id, team.AgeGroupId, team.AgeGroup?.Name ?? string.Empty, team.Name, team.ShortName, team.Club, team.ResponsiblePerson, team.Contact, team.IsActive, team.Players.Count);
    }
    
    public async Task<Guid> CreateAsync(Guid ageGroupId, string name, string shortName, string club, string responsiblePerson, string? contact, string? primaryColor, CancellationToken cancellationToken = default)
    {
        var ageGroup = await ageGroupRepository.GetAsync(ageGroupId, cancellationToken) ?? throw new InvalidOperationException("Age group was not found.");
        if (await teamRepository.ExistsInAgeGroupAsync(ageGroup.Id, name.Trim(), null, cancellationToken)) throw new InvalidOperationException("A team with the same name already exists in this age group.");
        var team = new Team { AgeGroupId = ageGroup.Id, Name = name.Trim(), ShortName = shortName.Trim(), Club = club.Trim(), ResponsiblePerson = responsiblePerson.Trim(), Contact = contact, PrimaryColor = primaryColor };
        team.Validate();
        await teamRepository.AddAsync(team, cancellationToken);
        await teamRepository.SaveChangesAsync(cancellationToken);
        return team.Id;
    }

    public async Task SetActiveAsync(Guid teamId, bool isActive, CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetAsync(teamId, cancellationToken) ?? throw new InvalidOperationException("Team was not found.");
        team.IsActive = isActive;
        team.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await teamRepository.SaveChangesAsync(cancellationToken);
    }
}
