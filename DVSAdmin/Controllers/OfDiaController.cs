using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    public class OfDiaController : Controller
    {
        [Route("ofdia-landing-page")]
        public IActionResult LandingPage()
        {
            return View();
        }
    }
}
