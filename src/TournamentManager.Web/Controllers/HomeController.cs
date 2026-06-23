using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TournamentManager.Web.Controllers;

[Authorize]
public sealed class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Tournaments");
    [AllowAnonymous]
    public IActionResult Error() => View();
}