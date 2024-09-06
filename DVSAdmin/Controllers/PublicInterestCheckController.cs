using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{

    [Route("public-interest-check")]
    [ValidCognitoToken]
    public class PublicInterestCheckController : Controller
    {
        private readonly ILogger<PublicInterestCheckController> logger;
        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IUserService userService;
        public PublicInterestCheckController(ILogger<PublicInterestCheckController> logger, IPublicInterestCheckService publicInterestCheckService, IUserService userService)
        {
            this.logger = logger;
            this.publicInterestCheckService = publicInterestCheckService;
            this.userService = userService;
        }

        [HttpGet("public-interest-check-list")]
        public async Task<IActionResult> PublicInterestCheck()
        {
        
                PublicInterestCheckViewModel publicInterestCheckViewModel = new PublicInterestCheckViewModel();

                var publicinterestchecks = await publicInterestCheckService.GetPICheckList();

                return View(publicInterestCheckViewModel);
           

        }
    }
}