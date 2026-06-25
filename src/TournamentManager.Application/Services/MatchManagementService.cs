using TournamentManager.Application.Abstractions;
using TournamentManager.Application.DTOs;
using TournamentManager.Domain.Entities;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Application.Services;

public sealed class MatchManagementService(IMatchRepository matchRepository, IPlayerRepository playerRepository)
{
    public async Task<MatchControlDto> GetControlAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        return ToControlDto(match);
    }

    public async Task StartAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        if (match.Status is not (MatchStatus.Scheduled or MatchStatus.Ready or MatchStatus.Paused)) throw new InvalidOperationException("Only scheduled, ready or paused matches can be started.");
        var now = DateTimeOffset.UtcNow;
        if (match.Status == MatchStatus.Paused && match.PausedAtUtc.HasValue)
        {
            match.TotalPausedSeconds += Math.Max(0, (long)(now - match.PausedAtUtc.Value).TotalSeconds);
            match.PausedAtUtc = null;
            if (match.PlannedPeriodCount > 1 && match.CurrentPeriodNumber < match.PlannedPeriodCount)
            {
                match.CurrentPeriodNumber++;
                match.NormalizeElapsedPlayingTimeToCurrentPeriodStart(now);
            }
        }
        match.Status = MatchStatus.InProgress;
        match.ActualStartUtc ??= now;
        match.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task PauseAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        if (match.Status != MatchStatus.InProgress) throw new InvalidOperationException("Only in-progress matches can be paused.");
        match.Status = MatchStatus.Paused;
        match.PausedAtUtc = DateTimeOffset.UtcNow;
        match.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task FinishAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        if (match.Status is not (MatchStatus.InProgress or MatchStatus.Paused)) throw new InvalidOperationException("Only started matches can be finished.");
        if (match.Status == MatchStatus.Paused && match.PausedAtUtc.HasValue)
        {
            var now = DateTimeOffset.UtcNow;
            match.TotalPausedSeconds += Math.Max(0, (long)(now - match.PausedAtUtc.Value).TotalSeconds);
            match.PausedAtUtc = null;
        }
        RecalculateScore(match);
        match.Status = MatchStatus.Finished;
        match.ActualEndUtc = DateTimeOffset.UtcNow;
        match.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterGoalAsync(Guid matchId, Guid teamId, Guid playerId, string userId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        if (match.Status != MatchStatus.InProgress) throw new InvalidOperationException("Goals can only be registered while the match is in progress.");
        if (teamId != match.HomeTeamId && teamId != match.AwayTeamId) throw new InvalidOperationException("The goal team must be one of the match teams.");
        var player = await playerRepository.GetAsync(playerId, cancellationToken) ?? throw new InvalidOperationException("Player was not found.");
        if (!player.IsActive) throw new InvalidOperationException("Inactive players cannot be selected for new match events.");
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId) throw new InvalidOperationException("The selected player must belong to one of the match teams.");
        var isOwnGoal = player.TeamId != teamId;
        var now = DateTimeOffset.UtcNow;
        var currentPeriodNumber = match.GetCurrentPeriodNumber(now);
        var goal = new GoalEvent { MatchId = matchId, Match = match, TeamId = teamId, PlayerId = playerId, MatchMinute = match.GetCurrentMatchMinute(now, currentPeriodNumber), MatchPeriodNumber = currentPeriodNumber, IsOwnGoal = isOwnGoal, RecordedByUserId = userId };
        goal.Validate();
        await matchRepository.AddGoalAsync(goal, cancellationToken);
        RecalculateScore(match);
        match.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task VoidGoalAsync(Guid goalEventId, CancellationToken cancellationToken = default)
    {
        var goal = await matchRepository.GetGoalAsync(goalEventId, cancellationToken) ?? throw new InvalidOperationException("Goal event was not found.");
        goal.IsActive = false;
        goal.UpdatedAtUtc = DateTimeOffset.UtcNow;
        if (goal.Match is not null) RecalculateScore(goal.Match);
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task SelectPlayerOfTheMatchAsync(Guid matchId, Guid teamId, Guid playerId, string userId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        if (teamId != match.HomeTeamId && teamId != match.AwayTeamId) throw new InvalidOperationException("The selected team must be one of the match teams.");
        var player = await playerRepository.GetAsync(playerId, cancellationToken) ?? throw new InvalidOperationException("Player was not found.");
        if (player.TeamId != teamId) throw new InvalidOperationException("Player of the match must belong to the selected team.");
        var vote = await matchRepository.GetVoteAsync(matchId, teamId, cancellationToken);
        if (vote is null)
        {
            vote = new PlayerOfTheMatchVote { MatchId = matchId, TeamId = teamId, PlayerId = playerId, SelectedByUserId = userId };
            vote.Validate();
            await matchRepository.AddVoteAsync(vote, cancellationToken);
        }
        else
        {
            vote.PlayerId = playerId;
            vote.SelectedByUserId = userId;
            vote.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task SelectGoalkeeperOfTheMatchAsync(Guid matchId, Guid playerId, string userId, CancellationToken cancellationToken = default)
    {
        var match = await GetMatchAsync(matchId, cancellationToken);
        var player = await playerRepository.GetAsync(playerId, cancellationToken) ?? throw new InvalidOperationException("Player was not found.");
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId) throw new InvalidOperationException("Goalkeeper of the match must belong to one of the match teams.");
        var vote = await matchRepository.GetGoalkeeperVoteAsync(matchId, cancellationToken);
        if (vote is null)
        {
            vote = new PlayerOfTheMatchVote { MatchId = matchId, TeamId = player.TeamId, PlayerId = playerId, SelectedByUserId = userId, IsGoalkeeperVote = true };
            vote.Validate();
            await matchRepository.AddVoteAsync(vote, cancellationToken);
        }
        else
        {
            vote.TeamId = player.TeamId;
            vote.PlayerId = playerId;
            vote.SelectedByUserId = userId;
            vote.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
        await matchRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Match> GetMatchAsync(Guid matchId, CancellationToken cancellationToken) => await matchRepository.GetForManagementAsync(matchId, cancellationToken) ?? throw new InvalidOperationException("Match was not found.");

    private static void RecalculateScore(Match match)
    {
        var homeGoals = match.GoalEvents.Count(x => x.IsActive && x.TeamId == match.HomeTeamId);
        var awayGoals = match.GoalEvents.Count(x => x.IsActive && x.TeamId == match.AwayTeamId);
        match.HomeScore = homeGoals;
        match.AwayScore = awayGoals;
    }

    private static MatchControlDto ToControlDto(Match match)
    {
        if (!match.HomeTeamId.HasValue || !match.AwayTeamId.HasValue) throw new InvalidOperationException("Match teams must be selected before managing the match.");
        RecalculateScore(match);
        var goals = match.GoalEvents.OrderBy(x => x.MatchPeriodNumber).ThenBy(x => x.MatchMinute).ThenBy(x => x.RecordedAtUtc).Select(x => new GoalEventDto(x.Id, x.MatchId, x.Team?.Name ?? string.Empty, x.Player?.DisplayName ?? string.Empty, x.MatchMinute, x.MatchPeriodNumber, match.FormatMatchMinute(x.MatchMinute, x.MatchPeriodNumber), x.IsOwnGoal, x.IsActive, x.RecordedAtUtc)).ToList();
        var homePlayers = match.HomeTeam?.Players.Where(x => x.IsActive).OrderBy(x => x.ShirtNumber).ThenBy(x => x.DisplayName).Select(x => new PlayerSelectionDto(x.Id, x.TeamId, x.DisplayName, x.ShirtNumber)).ToList() ?? new List<PlayerSelectionDto>();
        var awayPlayers = match.AwayTeam?.Players.Where(x => x.IsActive).OrderBy(x => x.ShirtNumber).ThenBy(x => x.DisplayName).Select(x => new PlayerSelectionDto(x.Id, x.TeamId, x.DisplayName, x.ShirtNumber)).ToList() ?? new List<PlayerSelectionDto>();
        return new MatchControlDto(match.Id, match.AgeGroupId, match.HomeTeamId!.Value, match.AwayTeamId!.Value, match.HomeTeam?.Name ?? string.Empty, match.AwayTeam?.Name ?? string.Empty, match.HomeScore ?? 0, match.AwayScore ?? 0, match.Status, match.ActualStartUtc, match.ActualEndUtc, (long)match.GetElapsedPlayingTime(DateTimeOffset.UtcNow).TotalSeconds, match.GetCurrentPeriodNumber(DateTimeOffset.UtcNow), match.PlannedDurationMinutes, match.PlannedPeriodCount, match.PlannedPeriodDurationMinutes, goals, homePlayers, awayPlayers, match.PlayerOfTheMatchVotes.FirstOrDefault(x => x.IsGoalkeeperVote)?.PlayerId);
    }
}
