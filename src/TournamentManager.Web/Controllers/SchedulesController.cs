using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.Schedules;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class SchedulesController(MatchScheduleService matchScheduleService, TeamService teamService, VenueService venueService, AgeGroupService ageGroupService) : Controller
{
    public async Task<IActionResult> Index(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        ViewBag.AgeGroup = await ageGroupService.GetEntityAsync(ageGroupId, cancellationToken);
        return View(await matchScheduleService.ListByAgeGroupAsync(ageGroupId, cancellationToken));
    }

    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> CreateManual(Guid ageGroupId, CancellationToken cancellationToken)
    {
        await PopulateManualMatchListsAsync(ageGroupId, cancellationToken);
        return View(new ManualMatchCreateViewModel { AgeGroupId = ageGroupId });
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateManual(ManualMatchCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateManualMatchListsAsync(model.AgeGroupId, cancellationToken);
            return View(model);
        }

        try
        {
            await matchScheduleService.CreateManualAsync(model.AgeGroupId, model.HomeTeamId, model.AwayTeamId, model.ScheduledStartUtc, model.VenueId, cancellationToken);
            return RedirectToAction(nameof(Index), new { ageGroupId = model.AgeGroupId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateManualMatchListsAsync(model.AgeGroupId, cancellationToken);
            return View(model);
        }
    }

    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> EditTeams(Guid id, Guid ageGroupId, CancellationToken cancellationToken)
    {
        await PopulateManualMatchListsAsync(ageGroupId, cancellationToken);
        return View(new MatchTeamsEditViewModel { MatchId = id, AgeGroupId = ageGroupId });
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTeams(MatchTeamsEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateManualMatchListsAsync(model.AgeGroupId, cancellationToken);
            return View(model);
        }
        try
        {
            await matchScheduleService.UpdateTeamsAsync(model.MatchId, model.HomeTeamId, model.AwayTeamId, cancellationToken);
            return RedirectToAction(nameof(Index), new { ageGroupId = model.AgeGroupId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateManualMatchListsAsync(model.AgeGroupId, cancellationToken);
            return View(model);
        }
    }

    private async Task PopulateManualMatchListsAsync(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.Teams = await teamService.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        ViewBag.Venues = await venueService.ListAsync(cancellationToken);
    }
}
