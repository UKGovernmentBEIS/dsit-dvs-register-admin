using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
   
    public class LoginController : Controller
    {
        
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
