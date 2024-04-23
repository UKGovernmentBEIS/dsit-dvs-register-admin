using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class LoginController : Controller
    {
        [HttpGet("")]
        public IActionResult SignUp()
        {
            return View("SignUp");
        }

        [HttpPost]
        public IActionResult CreateNewAccount()
        {
            return RedirectToAction("EnterEmailAddress", "Login");
        }

        [HttpGet("enter-email-address")]
        public IActionResult EnterEmailAddress()
        {
            return View("EnterEmailAddress");
        }

        //TODO: Add Sign in Action
    }
}
