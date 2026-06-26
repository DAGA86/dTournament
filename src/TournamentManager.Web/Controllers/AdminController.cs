using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.Application.Services;
using TournamentManager.Infrastructure.Identity;
using TournamentManager.Web.ViewModels.Admin;

namespace TournamentManager.Web.Controllers;

[Authorize(Policy = "AdministratorOnly")]
public sealed class AdminController(AuditService auditService, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> AuditLog(CancellationToken cancellationToken) => View(await auditService.ListRecentAsync(100, cancellationToken));

    public async Task<IActionResult> Operators(CancellationToken cancellationToken)
    {
        return View(await BuildOperatorsViewModelAsync(new OperatorPasswordViewModel(), cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeOperatorPassword(OperatorPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Operators", await BuildOperatorsViewModelAsync(model, cancellationToken));

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null || !await userManager.IsInRoleAsync(user, "Operator"))
        {
            ModelState.AddModelError(string.Empty, "A conta operator selecionada não existe.");
            return View("Operators", await BuildOperatorsViewModelAsync(model, cancellationToken));
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View("Operators", await BuildOperatorsViewModelAsync(model, cancellationToken));
        }

        TempData["OperatorPasswordChanged"] = $"Password da conta {user.Email ?? user.UserName} alterada com sucesso.";
        return RedirectToAction(nameof(Operators));
    }

    private async Task<OperatorAccountsViewModel> BuildOperatorsViewModelAsync(OperatorPasswordViewModel passwordForm, CancellationToken cancellationToken)
    {
        var operators = await userManager.GetUsersInRoleAsync("Operator");
        var operatorRows = operators
            .OrderBy(x => x.Email ?? x.UserName)
            .Select(x => new OperatorAccountViewModel(
                x.Id,
                x.Email ?? x.UserName ?? x.Id,
                x.DisplayName,
                x.LockoutEnd.HasValue && x.LockoutEnd.Value > DateTimeOffset.UtcNow))
            .ToList();

        if (string.IsNullOrWhiteSpace(passwordForm.UserId) && operatorRows.Count == 1)
        {
            passwordForm.UserId = operatorRows[0].UserId;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new OperatorAccountsViewModel(operatorRows, passwordForm);
    }
}
