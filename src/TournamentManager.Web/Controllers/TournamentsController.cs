using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.Tournaments;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class TournamentsController(TournamentService tournamentService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await tournamentService.ListAsync(cancellationToken));

    [Authorize(Policy = "AdministratorOnly")]
    public IActionResult Create() => View(new TournamentCreateViewModel());

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TournamentCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var id = await tournamentService.CreateAsync(model.Name, model.Description, model.Location, model.StartsOn, model.EndsOn, model.RulesNotes, cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var tournaments = await tournamentService.ListAsync(cancellationToken);
        var tournament = tournaments.FirstOrDefault(x => x.Id == id);
        if (tournament is null) return NotFound();
        return View(tournament);
    }
}
