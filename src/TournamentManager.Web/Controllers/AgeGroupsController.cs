using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.AgeGroups;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class AgeGroupsController(AgeGroupService ageGroupService) : Controller
{
    public async Task<IActionResult> Index(Guid tournamentId, CancellationToken cancellationToken) => View(await ageGroupService.ListByTournamentAsync(tournamentId, cancellationToken));

    [Authorize(Policy = "AdministratorOnly")]
    public IActionResult Create(Guid tournamentId) => View(new AgeGroupCreateViewModel { TournamentId = tournamentId });

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgeGroupCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await ageGroupService.CreateAsync(model.TournamentId, model.Name, model.BirthYearFrom, model.BirthYearTo, model.MatchDurationMinutes, model.CompetitionFormat, cancellationToken);
            return RedirectToAction(nameof(Index), new { tournamentId = model.TournamentId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
