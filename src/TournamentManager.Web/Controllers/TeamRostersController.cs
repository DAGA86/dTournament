using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.TeamRosters;

namespace TournamentManager.Web.Controllers;

[Route("equipas/{teamId:guid}/plantel")]
[Authorize(Policy = "OperatorOrAdministrator")]
public sealed class TeamRostersController(TeamRosterSubmissionService rosterService) : Controller
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Edit(Guid teamId, CancellationToken cancellationToken)
    {
        try { return View(await BuildViewModelAsync(teamId, cancellationToken)); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid teamId, TeamRosterSubmissionViewModel model, CancellationToken cancellationToken)
    {
        model.TeamId = teamId;
        try { await PopulateTeamNameAsync(model, cancellationToken: cancellationToken); }
        catch (InvalidOperationException) { return NotFound(); }
        ModelState.ClearValidationState(string.Empty);
        if (!TryValidateModel(model)) return View(model);
        try
        {
            await rosterService.SubmitAsync(teamId, model.Players.Select(x => (x.FullName, x.ShirtNumber, x.BirthDate)).ToList(), model.StaffMembers.Select(x => (x.FullName, x.Role)).ToList(), enforceSubmissionOpen: !CanManageRosterOutsideSubmissionWindow(), cancellationToken: cancellationToken);
            TempData["RosterSubmitted"] = "Plantel submetido com sucesso.";
            return RedirectToAction(nameof(Edit), new { teamId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private bool CanManageRosterOutsideSubmissionWindow() => User.IsInRole("Administrator") || User.IsInRole("Operator");
    
    private async Task<TeamRosterSubmissionViewModel> BuildViewModelAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var team = await rosterService.GetTeamAsync(teamId, enforceSubmissionOpen: !CanManageRosterOutsideSubmissionWindow(), cancellationToken: cancellationToken);
        var players = await rosterService.ListPlayersAsync(teamId, cancellationToken: cancellationToken);
        var staff = await rosterService.ListStaffAsync(teamId, cancellationToken: cancellationToken);
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
        var team = await rosterService.GetTeamAsync(model.TeamId, enforceSubmissionOpen: !CanManageRosterOutsideSubmissionWindow(), cancellationToken: cancellationToken);
        model.TeamName = team.Name;
        model.AgeGroupName = team.AgeGroupName;
        model.BirthYearFrom = team.BirthYearFrom;
        model.BirthYearTo = team.BirthYearTo;
    }
}