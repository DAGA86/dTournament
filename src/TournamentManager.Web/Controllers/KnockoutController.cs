using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class KnockoutController(KnockoutProgressionService knockoutProgressionService, AgeGroupService ageGroupService) : Controller
{
    public async Task<IActionResult> Index(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        ViewBag.AgeGroup = await ageGroupService.GetEntityAsync(ageGroupId, cancellationToken);
        return View(await knockoutProgressionService.ListFinalsAsync(ageGroupId, cancellationToken));
    }
}
