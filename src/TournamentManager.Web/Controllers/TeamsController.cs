using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.Teams;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class TeamsController(TeamService teamService) : Controller
{
    public async Task<IActionResult> Index(Guid ageGroupId, CancellationToken cancellationToken)
    {
        ViewBag.AgeGroupId = ageGroupId;
        return View(await teamService.ListByAgeGroupAsync(ageGroupId, cancellationToken));
    }

    [Authorize(Policy = "AdministratorOnly")]
    public IActionResult Create(Guid ageGroupId) => View(new TeamCreateViewModel { AgeGroupId = ageGroupId });

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeamCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await teamService.CreateAsync(model.AgeGroupId, model.Name, model.ShortName, model.Club, model.ResponsiblePerson, model.Contact, model.PrimaryColor, cancellationToken);
            return RedirectToAction(nameof(Index), new { ageGroupId = model.AgeGroupId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, Guid ageGroupId, bool isActive, CancellationToken cancellationToken)
    {
        await teamService.SetActiveAsync(id, isActive, cancellationToken);
        return RedirectToAction(nameof(Index), new { ageGroupId });
    }
}
