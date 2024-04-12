using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class OfDiaReviewController : Controller
    {
        [Route("")]
        [HttpGet("start-page")]
        public IActionResult StartPage()
        {
            return View();
        }


        [HttpGet("preregistration-review")]
        public IActionResult PreRegistrationReview()
        {
            return View();
        }


       
    }
}
