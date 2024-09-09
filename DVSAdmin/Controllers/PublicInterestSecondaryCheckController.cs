using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DVSAdmin.Controllers
{
    //[ValidCognitoToken]
    [Route("public-interest-secondary-check")]
    public class PublicInterestSecondaryCheckController : Controller
    {
        [Route("public-interest-secondary-check-review")]
        public IActionResult PublicInterestSecondaryCheck()
        {
            return View();
        }
    }
}
