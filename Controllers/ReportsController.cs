using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            // TODO: filter by sponsor if necessary
            return View();
        }
    }
}