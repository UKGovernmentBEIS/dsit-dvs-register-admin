using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("pre-registration-review")]
    public class PreRegistrationReviewController : Controller
    {
        private readonly ILogger<PreRegistrationReviewController> logger;
        private readonly IPreRegistrationReviewService preRegistrationReviewService;


        public PreRegistrationReviewController(ILogger<PreRegistrationReviewController> logger, IPreRegistrationReviewService preRegistrationReviewService)
        {
            this.logger = logger;
            this.preRegistrationReviewService = preRegistrationReviewService;
        }


      
        [HttpGet("pre-reg-at-a-glance")]
        public IActionResult PreRegAtAGlance()
        {
           return View();
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
