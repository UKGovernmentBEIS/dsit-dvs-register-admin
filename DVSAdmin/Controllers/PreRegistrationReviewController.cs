using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class PreRegistrationReviewController : Controller
    {
        private readonly ILogger<PreRegistrationReviewController> logger;


        public PreRegistrationReviewController(ILogger<PreRegistrationReviewController> logger)
        {
            this.logger = logger;

        }


        [Route("")]
        [HttpGet("pre-reg-at-a-glance")]
        public IActionResult PreRegAtAGlance()
        {
            //TODO:Populate view model with data frrom service layer
            PreRegAtAGlanceViewModel preRegAtAGlanceViewModel = new PreRegAtAGlanceViewModel
            { InReviewApplicationCount = 1,
            LessThanAWeekApplicationCount = 2,
            URNExpiredCount = 3,
            URNNotValidatedCount = 4}
            ;
            return View(preRegAtAGlanceViewModel);
        }

        [HttpGet("pre-registration-review")]
        public IActionResult PreRegistrationReview()
        {
            return View();
        }
    }
}
