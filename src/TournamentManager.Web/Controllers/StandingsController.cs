using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Domain.Enums;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class StandingsController(StandingsCalculationService standingsCalculationService, AgeGroupService ageGroupService, KnockoutProgressionService knockoutProgressionService) : Controller
{
    public async Task<IActionResult> Index(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        var ageGroup = await ageGroupService.GetEntityAsync(ageGroupId, cancellationToken);
        ViewBag.AgeGroup = ageGroup;
        if (ageGroup?.CompetitionFormat == CompetitionFormat.GroupStageAndFinals)
        {
            ViewBag.GroupStandings = await standingsCalculationService.CalculateByGroupAsync(ageGroupId, cancellationToken);
            ViewBag.FinalsStandings = await knockoutProgressionService.CalculateFinalTopFourAsync(ageGroupId, cancellationToken);
            return View(Array.Empty<TournamentManager.Application.DTOs.StandingDto>());
        }

        return View(await standingsCalculationService.CalculateAsync(ageGroupId, cancellationToken));
    }
}
