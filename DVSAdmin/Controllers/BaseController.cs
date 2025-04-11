using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class BaseController : Controller
    {
        public string UserEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;      
        public string UserProfile => HttpContext.Session.Get<string>("Profile") ?? string.Empty;

        protected string ControllerName => ControllerContext.ActionDescriptor.ControllerName;

        protected string ActionName => ControllerContext.ActionDescriptor.ActionName;

        public void SetRefererURL()
        {
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            ViewBag.RefererUrl = refererUrl;
        }
    }
}
