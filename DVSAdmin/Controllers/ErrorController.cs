using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("")]
    public class ErrorController : Controller
    {
        [HttpGet("service-error")]
        public IActionResult HandleException()
        {
            if (HttpContext != null)
            {
                HttpContext.Session.Clear();
                HttpContext.Response.Clear();
                HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            return View("ServiceIssue");
        }

        [HttpGet("page-not-found")]
        public IActionResult PageNotFound()
        {            
            return View("PageNotFound");
        }


    }
}
