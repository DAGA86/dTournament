using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;

namespace TournamentManager.Application.Services;

public sealed class TeamRosterSubmissionService(ITeamRepository teamRepository, IPlayerRepository playerRepository, ITeamStaffMemberRepository staffRepository, IMatchRepository matchRepository)
{
    public const int MaximumPlayers = 14;
    public const int MaximumStaffMembers = 3;

    public async Task<TeamRosterTeamDto> GetTeamAsync(Guid teamId, bool enforceSubmissionOpen = true, CancellationToken cancellationToken = default)
    {
        if (enforceSubmissionOpen && !await IsSubmissionOpenAsync(teamId, cancellationToken)) throw new InvalidOperationException("Roster submissions are closed for this team.");
        var team = await teamRepository.GetAsync(teamId, cancellationToken) ?? throw new InvalidOperationException("Team was not found.");
        var ageGroup = team.AgeGroup ?? throw new InvalidOperationException("Team age group was not loaded.");
        return new TeamRosterTeamDto(team.Id, team.Name, ageGroup.Name, ageGroup.BirthYearFrom, ageGroup.BirthYearTo);
    }

    public async Task<IReadOnlyList<PlayerDto>> ListPlayersAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        (await playerRepository.ListByTeamAsync(teamId, cancellationToken)).Select(p => new PlayerDto(p.Id, p.TeamId, p.Team?.Name ?? string.Empty, p.FullName, p.DisplayName, p.BirthDate, p.ShirtNumber, p.IsActive, p.AgeOverrideApproved)).ToList();

    public async Task<IReadOnlyList<TeamStaffMemberDto>> ListStaffAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        (await staffRepository.ListByTeamAsync(teamId, cancellationToken)).Select(s => new TeamStaffMemberDto(s.Id, s.TeamId, s.FullName, s.Role, s.IsActive)).ToList();

    public async Task<bool> IsSubmissionOpenAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var firstScheduledStartUtc = await matchRepository.GetFirstScheduledStartForTeamAsync(teamId, cancellationToken);
        if (!firstScheduledStartUtc.HasValue) return true;

        var submissionDeadlineUtc = firstScheduledStartUtc.Value.UtcDateTime.Date;
        return DateTimeOffset.UtcNow < new DateTimeOffset(submissionDeadlineUtc, TimeSpan.Zero);
    }

    public async Task SubmitAsync(Guid teamId, IReadOnlyList<(string? FullName, int? ShirtNumber, DateOnly? BirthDate)> playerEntries, IReadOnlyList<(string? FullName, string? Role)> staffEntries, bool enforceSubmissionOpen = true, CancellationToken cancellationToken = default)
    {
        if (enforceSubmissionOpen && !await IsSubmissionOpenAsync(teamId, cancellationToken)) throw new InvalidOperationException("Roster submissions are closed for this team.");
        var team = await teamRepository.GetAsync(teamId, cancellationToken) ?? throw new InvalidOperationException("Team was not found.");
        var players = playerEntries.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).Select(x => new Player { TeamId = team.Id, FullName = x.FullName!.Trim(), DisplayName = x.FullName!.Trim(), BirthDate = x.BirthDate ?? throw new InvalidOperationException("Player birth date is required."), ShirtNumber = x.ShirtNumber }).ToList();
        var staff = staffEntries.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).Select(x => new TeamStaffMember { TeamId = team.Id, FullName = x.FullName?.Trim() ?? string.Empty, Role = (x.Role ?? string.Empty).Trim() }).ToList();
        if (players.Count > MaximumPlayers) throw new InvalidOperationException($"A maximum of {MaximumPlayers} players can be submitted.");
        if (staff.Count > MaximumStaffMembers) throw new InvalidOperationException($"A maximum of {MaximumStaffMembers} staff members can be submitted.");
        if (players.Select(x => x.ShirtNumber).Where(x => x.HasValue).GroupBy(x => x!.Value).Any(x => x.Count() > 1)) throw new InvalidOperationException("Player shirt numbers must be unique within the team.");
        foreach (var player in players) player.Validate(team.AgeGroup ?? throw new InvalidOperationException("Team age group was not loaded."));
        foreach (var staffMember in staff) staffMember.Validate();
        await playerRepository.ReplaceForTeamAsync(team.Id, players, cancellationToken);
        await staffRepository.ReplaceForTeamAsync(team.Id, staff, cancellationToken);
        await staffRepository.SaveChangesAsync(cancellationToken);
    }
}