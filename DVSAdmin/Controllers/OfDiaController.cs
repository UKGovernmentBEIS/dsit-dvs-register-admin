using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class OfDiaController : Controller
    {
        [Route("ofdia-landing-page")]
        public IActionResult LandingPage()
        {          
            return View();
        }
    }
}
