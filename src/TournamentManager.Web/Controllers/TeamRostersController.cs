using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.TeamRosters;

namespace TournamentManager.Web.Controllers;

[Route("equipas/{teamId:guid}/plantel")]
public sealed class TeamRostersController(TeamRosterSubmissionService rosterService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Edit(Guid teamId, CancellationToken cancellationToken)
    {
        try { return View(await BuildViewModelAsync(teamId, cancellationToken)); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid teamId, TeamRosterSubmissionViewModel model, CancellationToken cancellationToken)
    {
        model.TeamId = teamId;
        await PopulateTeamNameAsync(model, cancellationToken);
        ModelState.ClearValidationState(string.Empty);
        if (!TryValidateModel(model)) return View(model);
        try
        {
            await rosterService.SubmitAsync(teamId, model.Players.Select(x => (x.FullName, x.ShirtNumber, x.BirthDate)).ToList(), model.StaffMembers.Select(x => (x.FullName, x.Role)).ToList(), cancellationToken);
            TempData["RosterSubmitted"] = "Plantel submetido com sucesso.";
            return RedirectToAction(nameof(Edit), new { teamId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private async Task<TeamRosterSubmissionViewModel> BuildViewModelAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var team = await rosterService.GetTeamAsync(teamId, cancellationToken);
        var players = await rosterService.ListPlayersAsync(teamId, cancellationToken);
        var staff = await rosterService.ListStaffAsync(teamId, cancellationToken);
        var model = new TeamRosterSubmissionViewModel
        {
            TeamId = teamId,
            TeamName = team.Name,
            AgeGroupName = team.AgeGroupName,
            BirthYearFrom = team.BirthYearFrom,
            BirthYearTo = team.BirthYearTo,
            Players = players.Select(x => new TeamRosterPlayerViewModel { FullName = x.FullName, ShirtNumber = x.ShirtNumber, BirthDate = x.BirthDate }).ToList(),
            StaffMembers = staff.Select(x => new TeamRosterStaffMemberViewModel { FullName = x.FullName, Role = x.Role }).ToList()
        };
        while (model.Players.Count < TeamRosterSubmissionService.MaximumPlayers) model.Players.Add(new TeamRosterPlayerViewModel());
        while (model.StaffMembers.Count < TeamRosterSubmissionService.MaximumStaffMembers) model.StaffMembers.Add(new TeamRosterStaffMemberViewModel());
        return model;
    }

    private async Task PopulateTeamNameAsync(TeamRosterSubmissionViewModel model, CancellationToken cancellationToken)
    {
        var team = await rosterService.GetTeamAsync(model.TeamId, cancellationToken);
        model.TeamName = team.Name;
        model.AgeGroupName = team.AgeGroupName;
        model.BirthYearFrom = team.BirthYearFrom;
        model.BirthYearTo = team.BirthYearTo;
    }
}