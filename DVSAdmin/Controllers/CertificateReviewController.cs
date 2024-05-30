using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("certificate-review")]
    public class CertificateReviewController : Controller
    {

        private readonly ILogger<CertificateReviewController> logger;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IUserService userService;
        public CertificateReviewController(ILogger<CertificateReviewController> logger, ICertificateReviewService certificateReviewService, IUserService userService)
        {
            this.logger = logger;
            this.certificateReviewService = certificateReviewService;
            this.userService = userService;
        }

        [HttpGet("certificate-review-list")]
        public async Task<ActionResult> CertificateReviews()
        {
            CertificateReviewListViewModel certificateReviewListViewModel = new CertificateReviewListViewModel();
            var certificateInfoList = await certificateReviewService.GetCertificateInformationList();
            certificateReviewListViewModel.CertificateReviewList = certificateInfoList.Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.Received
            ||  (x.CertificateReview !=null && x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.InReview)).ToList();            

            certificateReviewListViewModel.ArchiveList = certificateInfoList.Where( x=> x.CertificateReview !=null && (x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Expired 
            || x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Approved) ).ToList();
            return View(certificateReviewListViewModel);
        }

        [HttpGet("certificate-submissions")]
        public ActionResult CertificateSubmissions()
        {
            return View();
        }

        [HttpGet("archive-details")]
        public ActionResult ArchiveDetails()
        {
            return View();
        }

        [HttpGet("certificate-review")]
        public ActionResult CertificateReview()
        {
            return View();
        }

        //Temp route for building out view
        [HttpGet("certificate-review-submissions")]
        public ActionResult PartialViewsCertificateReviewSubmissionsView()
        {
            return View();
        }
        //Temp route for building out view
        [HttpGet("certificate-review-archive")]
        public ActionResult PartialViewsCertificateReviewArchiveView()
        {
            return View();
        }
    }
}
