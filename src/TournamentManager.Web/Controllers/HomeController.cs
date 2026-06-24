using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Domain.Enums;
using TournamentManager.Web.ViewModels;

namespace TournamentManager.Web.Controllers;

[AllowAnonymous]
public sealed class HomeController(
    AgeGroupService ageGroupService,
    TeamService teamService,
    PlayerService playerService,
    MatchScheduleService matchScheduleService,
    StandingsCalculationService standingsCalculationService) : Controller
{
    private static readonly Guid PublicTournamentId = Guid.Parse("e2ca8f96-1c45-4e28-9dee-286cbfba390c");

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var ageGroups = await ageGroupService.ListByTournamentAsync(PublicTournamentId, cancellationToken);
        return View(new PublicTournamentIndexViewModel(PublicTournamentId, ageGroups));
    }

    public async Task<IActionResult> AgeGroup(Guid id, CancellationToken cancellationToken)
    {
        var ageGroup = (await ageGroupService.ListByTournamentAsync(PublicTournamentId, cancellationToken)).FirstOrDefault(x => x.Id == id);
        if (ageGroup is null) return NotFound();
        return View(new PublicAgeGroupViewModel(PublicTournamentId, ageGroup));
    }

    public async Task<IActionResult> LiveMatches(Guid ageGroupId, CancellationToken cancellationToken)
    {
        if (!await IsPublicAgeGroupAsync(ageGroupId, cancellationToken)) return NotFound();
        ViewBag.AgeGroupId = ageGroupId;
        ViewBag.PublicView = true;
        var matches = await matchScheduleService.ListByAgeGroupAsync(ageGroupId, cancellationToken);
        return View("~/Views/Schedules/Index.cshtml", matches.Where(x => x.Status is MatchStatus.InProgress or MatchStatus.Paused).ToList());
    }

    public async Task<IActionResult> Schedule(Guid ageGroupId, CancellationToken cancellationToken)
    {
        if (!await IsPublicAgeGroupAsync(ageGroupId, cancellationToken)) return NotFound();
        ViewBag.AgeGroupId = ageGroupId;
        ViewBag.PublicView = true;
        return View("~/Views/Schedules/Index.cshtml", await matchScheduleService.ListByAgeGroupAsync(ageGroupId, cancellationToken));
    }

    public async Task<IActionResult> Standings(Guid ageGroupId, CancellationToken cancellationToken)
    {
        if (!await IsPublicAgeGroupAsync(ageGroupId, cancellationToken)) return NotFound();
        ViewBag.AgeGroupId = ageGroupId;
        ViewBag.PublicView = true;
        return View("~/Views/Standings/Index.cshtml", await standingsCalculationService.CalculateAsync(ageGroupId, cancellationToken));
    }

    public async Task<IActionResult> Team(Guid id, CancellationToken cancellationToken)
    {
        var team = await teamService.GetAsync(id, cancellationToken);
        if (team is null) return NotFound();

        var ageGroup = (await ageGroupService.ListByTournamentAsync(PublicTournamentId, cancellationToken)).FirstOrDefault(x => x.Id == team.AgeGroupId);
        if (ageGroup is null) return NotFound();

        var matches = await matchScheduleService.ListByTeamAsync(id, cancellationToken);

        var players = await playerService.ListByTeamAsync(id, cancellationToken);
        var finishedMatches = await matchScheduleService.ListFinishedGoalEventsByTeamAsync(id, cancellationToken);
        var goalsByPlayer = finishedMatches.SelectMany(x => x.GoalEvents).Where(x => x.TeamId == id && x.IsActive && !x.IsOwnGoal).GroupBy(x => x.PlayerId).ToDictionary(x => x.Key, x => x.Count());
        var playerSummaries = players.Select(x => new PlayerGoalSummaryViewModel(x.Id, x.DisplayName, x.FullName, x.ShirtNumber, goalsByPlayer.GetValueOrDefault(x.Id))).ToList();
        return View(new PublicTeamDetailViewModel(team, playerSummaries, matches));
    }

    private async Task<bool> IsPublicAgeGroupAsync(Guid ageGroupId, CancellationToken cancellationToken) =>
        (await ageGroupService.ListByTournamentAsync(PublicTournamentId, cancellationToken)).Any(x => x.Id == ageGroupId);

    public IActionResult Error() => View();
}
