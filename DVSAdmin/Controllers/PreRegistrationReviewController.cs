using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    
    [Route("pre-registration-review")]
    [ValidCognitoToken]
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
            var preregistrations = await preRegistrationReviewService.GetPreRegistrations();
            preRegReviewListViewModel.PrimaryChecksList = preregistrations.Where(x=> x.DaysLeftToComplete>0 ).ToList();

            preRegReviewListViewModel.SecondaryChecksList = preregistrations
            .Where(x => x.PreRegistrationReview !=null  && x.DaysLeftToComplete>0   &&(x.PreRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckApproved ||
            x.PreRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckRejected )
            /*&& x.PreRegistrationReview.PrimaryCheckUserId !=*/ ).ToList(); //To Do filter based on current user

            //TODO: get data brom service
            preRegReviewListViewModel.ArchiveList = new List<UniqueReferenceNumberDto>();
            preRegReviewListViewModel.ArchiveList.Add(new UniqueReferenceNumberDto { URN = "Regis-220424-YFN-463-200", RegisteredDIPName = "Trading name", ReleasedTimeStamp = DateTime.UtcNow, URNStatus = URNStatusEnum.Expired });
            return View(preRegReviewListViewModel);
        }
    }
}
