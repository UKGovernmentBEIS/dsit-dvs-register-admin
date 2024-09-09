using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using DVSRegister.Extensions;
using DVSAdmin.CommonUtility;


namespace DVSAdmin.Controllers
{
    [Route("public-interest-primary-check")]
    public class PublicInterestPrimaryCheckController : Controller
    {
        [HttpGet("application-reivew")]
        public IActionResult ApplicationReview()
        {
            return View();

        }
    }
}
