using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    public class BaseController : Controller
    {
        public string UserEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;
        public void SetRefererURL()
        {
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            ViewBag.RefererUrl = refererUrl;
        }
    }
}
