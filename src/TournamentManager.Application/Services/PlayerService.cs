using System.Numerics;
using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository)
{
    public async Task<IReadOnlyList<PlayerDto>> ListByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var players = await playerRepository.ListByTeamAsync(teamId, cancellationToken);
        return players.Select(p => new PlayerDto(p.Id, p.TeamId, p.Team?.Name ?? string.Empty, p.FullName, p.DisplayName, p.BirthDate, p.ShirtNumber, p.IsActive, p.AgeOverrideApproved)).ToList();
    }

    public async Task<Guid> CreateAsync(Guid teamId, string fullName, string displayName, DateOnly birthDate, int? shirtNumber, bool ageOverrideApproved, string? ageOverrideReason, string? approvedByUserId, CancellationToken cancellationToken = default)
    {
        var team = await teamRepository.GetAsync(teamId, cancellationToken) ?? throw new InvalidOperationException("Team was not found.");
        if (shirtNumber.HasValue && await playerRepository.ShirtNumberExistsAsync(team.Id, shirtNumber.Value, null, cancellationToken)) throw new InvalidOperationException("A player with the same shirt number already exists in this team.");
        var player = new Player { TeamId = team.Id, FullName = fullName.Trim(), DisplayName = displayName.Trim(), BirthDate = birthDate, ShirtNumber = shirtNumber, AgeOverrideApproved = ageOverrideApproved, AgeOverrideReason = ageOverrideReason, AgeOverrideApprovedByUserId = approvedByUserId };
        player.Validate(team.AgeGroup ?? throw new InvalidOperationException("Team age group was not loaded."));
        await playerRepository.AddAsync(player, cancellationToken);
        await playerRepository.SaveChangesAsync(cancellationToken);
        return player.Id;
    }

    public async Task SetActiveAsync(Guid playerId, bool isActive, CancellationToken cancellationToken = default)
    {
        var player = await playerRepository.GetAsync(playerId, cancellationToken) ?? throw new InvalidOperationException("Player was not found.");
        player.IsActive = isActive;
        player.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await playerRepository.SaveChangesAsync(cancellationToken);
    }
}