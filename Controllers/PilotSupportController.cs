using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "admin")]
public class PilotSupportController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult DemoAccounts()
    {
        return View();
    }

    public IActionResult TestData()
    {
        return View();
    }

    public IActionResult UATScripts()
    {
        return View();
    }

    public IActionResult PilotChecklist()
    {
        return View();
    }

    public IActionResult Documentation()
    {
        return View();
    }
}
