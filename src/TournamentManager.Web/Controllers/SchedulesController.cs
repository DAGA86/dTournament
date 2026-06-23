using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.Schedules;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class SchedulesController(MatchScheduleService matchScheduleService) : Controller
{
    public async Task<IActionResult> Index(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        return View(await matchScheduleService.ListByAgeGroupAsync(ageGroupId, cancellationToken));
    }

    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Preview(Guid ageGroupId, CancellationToken cancellationToken)
    {
        try
        {
            ViewBag.AgeGroupId = ageGroupId;
            return View(await matchScheduleService.PreviewAsync(ageGroupId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ScheduleError"] = ex.Message;
            return RedirectToAction(nameof(Index), new { ageGroupId });
        }
    }

    [Authorize(Policy = "AdministratorOnly")]
    public IActionResult Generate(Guid ageGroupId) => View(new ScheduleGenerateViewModel { AgeGroupId = ageGroupId });

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(ScheduleGenerateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await matchScheduleService.GenerateAsync(model.AgeGroupId, model.FirstKickoffUtc, model.MinutesBetweenMatches, model.VenueId, cancellationToken);
            return RedirectToAction(nameof(Index), new { ageGroupId = model.AgeGroupId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
