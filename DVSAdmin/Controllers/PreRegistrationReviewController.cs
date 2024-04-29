using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    
    [Route("pre-registration-review")]
    //[ValidCognitoToken]
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

        /// <summary>
        /// Load Landing page with grid having
        /// primary check, secondary check and archive List
        /// </summary>
        /// <returns></returns>
        [HttpGet("pre-registration-review")]
        public async Task<IActionResult> PreRegistrationReview()
        {
            PreRegReviewListViewModel preRegReviewListViewModel = new PreRegReviewListViewModel();
            var preregistrations = await preRegistrationReviewService.GetPreRegistrations();
            preRegReviewListViewModel.PrimaryChecksList = preregistrations.Where(x => (x.ApplicationReviewStatus == ApplicationReviewStatusEnum.Received && x.Id !=x?.PreRegistrationReview?.PreRegistrationId)||
            x?.PreRegistrationReview?.ApplicationReviewStatus == ApplicationReviewStatusEnum.InPrimaryReview
            ||  x?.PreRegistrationReview?.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckPassed
            ||  x?.PreRegistrationReview?.ApplicationReviewStatus ==ApplicationReviewStatusEnum.PrimaryCheckFailed
            ||  x?.PreRegistrationReview?.ApplicationReviewStatus ==ApplicationReviewStatusEnum.SentBackBySecondReviewer
            ).ToList();

            preRegReviewListViewModel.SecondaryChecksList = preregistrations
            .Where(x => x.PreRegistrationReview !=null    && x.DaysLeftToComplete>0
            &&(x.PreRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckPassed ||
            x.PreRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.PrimaryCheckFailed ||
            x.PreRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationApproved ||
            x.PreRegistrationReview.ApplicationReviewStatus == ApplicationReviewStatusEnum.ApplicationRejected)
            /*&& x.PreRegistrationReview.PrimaryCheckUserId !=*/ ).ToList(); //To Do filter based on current user



            preRegReviewListViewModel.ArchiveList = preregistrations
            .Where(x => x.UniqueReferenceNumber !=null && (x.UniqueReferenceNumber.URNStatus == URNStatusEnum.Rejected
            || x.UniqueReferenceNumber.URNStatus == URNStatusEnum.Approved || x.UniqueReferenceNumber.URNStatus == URNStatusEnum.ValidatedByCAB ||
            x.UniqueReferenceNumber.URNStatus == URNStatusEnum.Rejected || x.UniqueReferenceNumber.URNStatus == URNStatusEnum.Expired)).ToList();
            return View(preRegReviewListViewModel);
        }


        /// <summary>
        /// Review screen with approve/reject sections
        /// </summary>
        /// <param name="preRegistrationId">The pre registration identifier.</param>
        /// <returns></returns>
        [HttpGet("archive-details")]
        public async Task<IActionResult> ArchiveDetails(int preRegistrationId)
        {
            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();

            PreRegistrationDto preRegistrationDto = await preRegistrationReviewService.GetPreRegistration(preRegistrationId);
            preRegistrationReviewViewModel = MapDtoToViewModel(preRegistrationDto);

            return View(preRegistrationReviewViewModel);


        }

        public static PreRegistrationReviewViewModel MapDtoToViewModel(PreRegistrationDto preRegistrationDto)
        {

            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();
            preRegistrationReviewViewModel.PreRegistration = preRegistrationDto;

            if (preRegistrationDto.PreRegistrationReview!= null)
            {
                preRegistrationReviewViewModel.PreRegistration = preRegistrationDto;
                preRegistrationReviewViewModel.IsCountryApproved = preRegistrationDto.PreRegistrationReview.IsCountryApproved;
                preRegistrationReviewViewModel.IsCompanyApproved=  preRegistrationDto.PreRegistrationReview.IsCompanyApproved;
                preRegistrationReviewViewModel.IsCheckListApproved= preRegistrationDto.PreRegistrationReview.IsCheckListApproved;
                preRegistrationReviewViewModel.IsDirectorshipsApproved = preRegistrationDto.PreRegistrationReview.IsDirectorshipsApproved;
                preRegistrationReviewViewModel.IsDirectorshipsAndRelationApproved= preRegistrationDto.PreRegistrationReview.IsDirectorshipsAndRelationApproved;
                preRegistrationReviewViewModel.IsTradingAddressApproved= preRegistrationDto.PreRegistrationReview.IsTradingAddressApproved;
                preRegistrationReviewViewModel.IsSanctionListApproved= preRegistrationDto.PreRegistrationReview.IsSanctionListApproved;
                preRegistrationReviewViewModel.IsUNFCApproved= preRegistrationDto.PreRegistrationReview.IsUNFCApproved;
                preRegistrationReviewViewModel.IsECCheckApproved= preRegistrationDto.PreRegistrationReview.IsECCheckApproved;
                preRegistrationReviewViewModel.IsTARICApproved= preRegistrationDto.PreRegistrationReview.IsTARICApproved;
                preRegistrationReviewViewModel.IsBannedPoliticalApproved= preRegistrationDto.PreRegistrationReview.IsBannedPoliticalApproved;
                preRegistrationReviewViewModel.IsProvidersWebpageApproved= preRegistrationDto.PreRegistrationReview.IsProvidersWebpageApproved;
                preRegistrationReviewViewModel.Comment = preRegistrationDto.PreRegistrationReview.Comment;
                preRegistrationReviewViewModel.ApplicationReviewStatus = preRegistrationDto.PreRegistrationReview.ApplicationReviewStatus;
            }
            return preRegistrationReviewViewModel;
        }

    }
}
