using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class ErrorController : Controller
    {
        [HttpGet("pre-registration-review/service-error")]
        public IActionResult PreRegHandleException()
        {
            HttpContext.Session.Clear();
            return View("ServiceIssue");
        }
        [HttpGet("certificate-review/service-error")]
        public IActionResult CertReviewHandleException()
        {
            HttpContext.Session.Clear();
            return View("ServiceIssue");
        }
    }
}
