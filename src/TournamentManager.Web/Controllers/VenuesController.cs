using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Web.ViewModels.Venues;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AuthenticatedViewer")]
public sealed class VenuesController(VenueService venueService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await venueService.ListAsync(cancellationToken));

    [Authorize(Policy = "AdministratorOnly")]
    public IActionResult Create() => View(new VenueCreateViewModel());

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VenueCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await venueService.CreateAsync(model.Name, model.Description, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost, Authorize(Policy = "AdministratorOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        await venueService.SetActiveAsync(id, isActive, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
