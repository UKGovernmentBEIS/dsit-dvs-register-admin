using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class ErrorController : Controller
    {
        [HttpGet("service-error")]
        public IActionResult HandleException()
        {
            HttpContext.Session.Clear();
            return View("ServiceIssue");
        }
    }
}
