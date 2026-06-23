using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AdministratorOnly")]
public sealed class AdminController(AuditService auditService) : Controller
{
    public async Task<IActionResult> AuditLog(CancellationToken cancellationToken) => View(await auditService.ListRecentAsync(100, cancellationToken));
}
