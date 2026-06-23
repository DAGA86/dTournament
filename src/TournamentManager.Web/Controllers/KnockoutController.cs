using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class KnockoutController(QualificationService qualificationService, KnockoutProgressionService knockoutProgressionService) : Controller
{
    public async Task<IActionResult> Index(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        return View(await knockoutProgressionService.ListFinalsAsync(ageGroupId, cancellationToken));
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSemiFinals(Guid ageGroupId, CancellationToken cancellationToken)
    {
        await qualificationService.CreateSemiFinalsAsync(ageGroupId, cancellationToken);
        return RedirectToAction(nameof(Index), new { ageGroupId });
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ProgressFinals(Guid ageGroupId, CancellationToken cancellationToken)
    {
        await knockoutProgressionService.ProgressAfterSemiFinalsAsync(ageGroupId, cancellationToken);
        return RedirectToAction(nameof(Index), new { ageGroupId });
    }
}
