using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class BaseController : Controller
    {
        public string UserEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;      
        public string UserProfile => HttpContext.Session.Get<string>("Profile") ?? string.Empty;
        public void SetRefererURL()
        {
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            ViewBag.RefererUrl = refererUrl;
        }
    }
}
