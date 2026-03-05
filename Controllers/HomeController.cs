using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Offline()
        {
            return View();
        }
    }
}