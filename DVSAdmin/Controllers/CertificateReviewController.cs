using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("certificate-review")]
    public class CertificateReviewController : Controller
    {
        [HttpGet("certificate-review-list")]
        public ActionResult CertificateReviews()
        {
            return View();
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
    }
}
