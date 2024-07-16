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

        [HttpGet("service-error")]
        public IActionResult HandleException()
        {
            HttpContext.Session.Clear();
            return View("ServiceIssue");
        }

        [HttpGet("register-management/service-error")]
        public IActionResult RegManagementHandleException()
        {
            HttpContext.Session.Clear();
            return View("RegManagementError");
        }

        [HttpGet("consent/service-error")]
        public IActionResult ConsentHandleException()
        {
            HttpContext.Session.Clear();
            return View("ConsentError");
        }
    }
}
