using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class ErrorController : Controller
    {
        [HttpGet("preregistration-review/service-error")]
        public IActionResult HandleException()
        {
            HttpContext.Session.Clear();
            return View("ServiceIssue");
        }
    }
}
