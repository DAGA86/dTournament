using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.AgeGroups;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class AgeGroupsController(AgeGroupService ageGroupService, VenueService venueService) : Controller
{
    public async Task<IActionResult> Index(Guid tournamentId, CancellationToken cancellationToken)
    {
        ViewBag.TournamentId = tournamentId;
        return View(await ageGroupService.ListByTournamentAsync(tournamentId, cancellationToken));
    }

    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Create(Guid tournamentId, CancellationToken cancellationToken)
    {
        await PopulateCreateListsAsync(cancellationToken);
        return View(new AgeGroupCreateViewModel { TournamentId = tournamentId });
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgeGroupCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateListsAsync(cancellationToken);
            return View(model);
        }
        try
        {
            var plannedMatches = model.PlannedMatches.Select(x => new PlannedMatchInput(x.RoundNumber, x.Phase, x.ScheduledStartUtc, x.VenueId)).ToList();
            await ageGroupService.CreateAsync(model.TournamentId, model.Name, model.BirthYearFrom, model.BirthYearTo, model.MatchDurationMinutes, model.NumberOfPeriods, model.HalfTimeBreakMinutes, model.CompetitionFormat, model.GroupCount, model.FinalsStartPhase, plannedMatches, cancellationToken);
            return RedirectToAction(nameof(Index), new { tournamentId = model.TournamentId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateListsAsync(cancellationToken);
            return View(model);
        }
    }

    private async Task PopulateCreateListsAsync(CancellationToken cancellationToken)
    {
        ViewBag.Venues = await venueService.ListAsync(cancellationToken);
    }
}
