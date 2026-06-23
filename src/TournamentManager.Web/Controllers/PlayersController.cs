using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.Players;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class PlayersController(PlayerService playerService) : Controller
{
    public async Task<IActionResult> Index(Guid teamId, CancellationToken cancellationToken) => View(await playerService.ListByTeamAsync(teamId, cancellationToken));

    [Authorize(Policy = "AdministratorOnly")]
    public IActionResult Create(Guid teamId) => View(new PlayerCreateViewModel { TeamId = teamId });

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlayerCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await playerService.CreateAsync(model.TeamId, model.FullName, model.DisplayName, model.BirthDate, model.ShirtNumber, model.AgeOverrideApproved, model.AgeOverrideReason, userId, cancellationToken);
            return RedirectToAction(nameof(Index), new { teamId = model.TeamId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, Guid teamId, bool isActive, CancellationToken cancellationToken)
    {
        await playerService.SetActiveAsync(id, isActive, cancellationToken);
        return RedirectToAction(nameof(Index), new { teamId });
    }
}
