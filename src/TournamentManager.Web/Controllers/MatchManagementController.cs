using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TournamentManager.Application.Services;
using TournamentManager.Web.Hubs;
using TournamentManager.Web.ViewModels.MatchManagement;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "OperatorOrAdministrator")]
public sealed class MatchManagementController(MatchManagementService matchManagementService, AuditService auditService, IHubContext<TournamentUpdatesHub> hubContext) : Controller
{
    public async Task<IActionResult> Control(Guid id, CancellationToken cancellationToken) => View(await matchManagementService.GetControlAsync(id, cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        await matchManagementService.StartAsync(id, cancellationToken);
        await auditService.RecordAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "StartMatch", "Match", id.ToString(), null, "InProgress", null, cancellationToken);
        await NotifyMatchChangedAsync(id, "matchStarted", cancellationToken);
        return RedirectToAction(nameof(Control), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pause(Guid id, CancellationToken cancellationToken)
    {
        await matchManagementService.PauseAsync(id, cancellationToken);
        await auditService.RecordAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "PauseMatch", "Match", id.ToString(), null, "Paused", null, cancellationToken);
        await NotifyMatchChangedAsync(id, "matchPaused", cancellationToken);
        return RedirectToAction(nameof(Control), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finish(Guid id, CancellationToken cancellationToken)
    {
        await matchManagementService.FinishAsync(id, cancellationToken);
        await auditService.RecordAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "FinishMatch", "Match", id.ToString(), null, "Finished", null, cancellationToken);
        await NotifyMatchChangedAsync(id, "matchFinished", cancellationToken);
        return RedirectToAction(nameof(Control), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterGoal(RegisterGoalViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Control), new { id = model.MatchId });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Authenticated user id was not found.");
        await matchManagementService.RegisterGoalAsync(model.MatchId, model.TeamId, model.PlayerId, model.MatchMinute, model.IsOwnGoal, userId, cancellationToken);
        await auditService.RecordAsync(userId, "RegisterGoal", "Match", model.MatchId.ToString(), null, $"Goal:{model.TeamId}:{model.PlayerId}:{model.MatchMinute}", null, cancellationToken);
        await NotifyMatchChangedAsync(model.MatchId, "goalRegistered", cancellationToken);
        return RedirectToAction(nameof(Control), new { id = model.MatchId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VoidGoal(Guid id, Guid matchId, CancellationToken cancellationToken)
    {
        await matchManagementService.VoidGoalAsync(id, cancellationToken);
        await auditService.RecordAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "VoidGoal", "GoalEvent", id.ToString(), "Active", "Voided", null, cancellationToken);
        await NotifyMatchChangedAsync(matchId, "goalVoided", cancellationToken);
        return RedirectToAction(nameof(Control), new { id = matchId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectPlayerOfTheMatch(PlayerOfTheMatchViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Control), new { id = model.MatchId });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Authenticated user id was not found.");
        await matchManagementService.SelectPlayerOfTheMatchAsync(model.MatchId, model.TeamId, model.PlayerId, userId, cancellationToken);
        await auditService.RecordAsync(userId, "SelectPlayerOfTheMatch", "Match", model.MatchId.ToString(), null, $"{model.TeamId}:{model.PlayerId}", null, cancellationToken);
        await NotifyMatchChangedAsync(model.MatchId, "playerOfTheMatchSelected", cancellationToken);
        return RedirectToAction(nameof(Control), new { id = model.MatchId });
    }

    private Task NotifyMatchChangedAsync(Guid matchId, string eventName, CancellationToken cancellationToken) => hubContext.Clients.Group(TournamentUpdatesHub.MatchGroup(matchId.ToString())).SendAsync("MatchChanged", new { matchId, eventName }, cancellationToken);
}
