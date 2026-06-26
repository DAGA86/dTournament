using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.AgeGroups;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class AgeGroupsController(
    AgeGroupService ageGroupService,
    VenueService venueService,
    TournamentService tournamentService,
    ILogger<AgeGroupsController> logger) : Controller
{
    public async Task<IActionResult> Index(Guid tournamentId, CancellationToken cancellationToken)
    {
        ViewBag.TournamentId = tournamentId;
        return View(await ageGroupService.ListByTournamentAsync(tournamentId, cancellationToken));
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDisplayOrder(Guid id, Guid tournamentId, int displayOrder, CancellationToken cancellationToken)
    {
        await ageGroupService.SetDisplayOrderAsync(id, displayOrder, cancellationToken);
        return RedirectToAction(nameof(Index), new { tournamentId });
    }
    
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Create(Guid tournamentId, CancellationToken cancellationToken)
    {
        await PopulateCreateListsAsync(tournamentId, cancellationToken);
        return View(new AgeGroupCreateViewModel { TournamentId = tournamentId });
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgeGroupCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateListsAsync(model.TournamentId, cancellationToken);
            return View(model);
        }
        try
        {
            var plannedMatches = model.PlannedMatches.Select(x => new PlannedMatchInput(x.Phase, x.GroupDisplayOrder, x.ScheduledStartUtc, x.VenueId)).ToList();
            await ageGroupService.CreateAsync(model.TournamentId, model.Name, model.BirthYearFrom, model.BirthYearTo, model.MatchDurationMinutes, model.NumberOfPeriods, model.NumberOfPeriods == 1 ? 0 : model.HalfTimeBreakMinutes, model.CompetitionFormat, model.GroupCount, model.FinalsStartPhase, plannedMatches, cancellationToken);
            return RedirectToAction(nameof(Index), new { tournamentId = model.TournamentId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateListsAsync(model.TournamentId, cancellationToken);
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating age group {AgeGroupName} for tournament {TournamentId} with {PlannedMatchCount} planned matches.", model.Name, model.TournamentId, model.PlannedMatches.Count);
            ModelState.AddModelError(string.Empty, "Não foi possível criar o escalão. Confirme os dados dos jogos e tente novamente; se o problema persistir, contacte o administrador.");
            await PopulateCreateListsAsync(model.TournamentId, cancellationToken);
            return View(model);
        }
    }

    private async Task PopulateCreateListsAsync(Guid tournamentId, CancellationToken cancellationToken)
    {
        ViewBag.TournamentStartsOn = (await tournamentService.GetAsync(tournamentId, cancellationToken))?.StartsOn;
        ViewBag.Venues = await venueService.ListAsync(cancellationToken);
    }
}
