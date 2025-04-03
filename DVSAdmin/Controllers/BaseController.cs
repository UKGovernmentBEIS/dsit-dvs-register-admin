using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    //[ValidCognitoToken]
    public class BaseController : Controller
    {

        //public string UserEmail => "joe.lauria@dsit.gov.uk";
        public string UserEmail => "joe.lauria@uk.ey.com";
        public string UserProfile => "DSIT";
        public void SetRefererURL()
        {
            string refererUrl = HttpContext.Request.Headers["Referer"].ToString();
            ViewBag.RefererUrl = refererUrl;
        }
    }
}
