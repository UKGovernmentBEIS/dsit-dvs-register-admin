using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class PreRegistrationReviewController : Controller
    {
        private readonly ILogger<PreRegistrationReviewController> logger;
        private readonly IPreRegistrationReviewService preRegistrationReviewService;


        public PreRegistrationReviewController(ILogger<PreRegistrationReviewController> logger, IPreRegistrationReviewService preRegistrationReviewService)
        {
            this.logger = logger;
            this.preRegistrationReviewService = preRegistrationReviewService;
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
        public async Task<IActionResult> PreRegistrationReview()
        {
            PreRegReviewListViewModel preRegReviewListViewModel = new PreRegReviewListViewModel();
            preRegReviewListViewModel.PreRegistrations = await preRegistrationReviewService.GetPreRegistrations();
            return View(preRegReviewListViewModel);
        }
    }
}
