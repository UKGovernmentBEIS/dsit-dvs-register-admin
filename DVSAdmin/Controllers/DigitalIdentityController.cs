using Microsoft.AspNetCore.Mvc;
using DVSRegister.Extensions;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class DigitalIdentityController : Controller
    {
        [Route("home")]
        public IActionResult LandingPage()
        {
            return View();
        }
    }
}
