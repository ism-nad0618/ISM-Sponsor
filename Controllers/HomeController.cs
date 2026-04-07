using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Offline()
        {
            return View();
        }

        /// <summary>
        /// Demo page for testing UI/UX components
        /// Route: /Home/UiDemo
        /// </summary>
        public IActionResult UiDemo()
        {
            return View();
        }
    }
}