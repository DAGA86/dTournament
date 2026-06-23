using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TournamentManager.Web.Hubs;

[Authorize]
public sealed class TournamentUpdatesHub : Hub
{
    public Task JoinMatchGroup(string matchId) => Groups.AddToGroupAsync(Context.ConnectionId, MatchGroup(matchId));
    public Task LeaveMatchGroup(string matchId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, MatchGroup(matchId));
    public static string MatchGroup(string matchId) => $"match:{matchId}";
}
