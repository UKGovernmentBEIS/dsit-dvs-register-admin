using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("preregistration-review/service-error")]
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult HandleException()
        {           
            return View("ServiceIssue");
        }

    }
}
