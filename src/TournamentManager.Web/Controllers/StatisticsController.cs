using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class StatisticsController(PlayerStatisticsService playerStatisticsService) : Controller
{
    public async Task<IActionResult> Players(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        return View(await playerStatisticsService.GetTopPlayersByAgeGroupAsync(ageGroupId, cancellationToken));
    }

    public async Task<IActionResult> Teams(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        return View(await playerStatisticsService.GetTeamStatisticsByAgeGroupAsync(ageGroupId, cancellationToken));
    }
}
