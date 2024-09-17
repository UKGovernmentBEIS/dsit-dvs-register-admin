using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
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

            string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");

            if (!string.IsNullOrEmpty(loggedinUserEmail))
            {
                UserDto userDto = await userService.GetUser(loggedinUserEmail);
                PublicInterestCheckViewModel publicInterestCheckViewModel = new PublicInterestCheckViewModel();

                var publicinterestchecks = await publicInterestCheckService.GetPICheckList();

                publicInterestCheckViewModel.PrimaryChecksList = publicinterestchecks.Where(x => x.DaysLeftToComplete > 0).
                Where(x => (x.ServiceStatus == ServiceStatusEnum.Received && x.Id != x?.PublicInterestCheck?.ServiceId) ||
                (x?.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.InPrimaryReview               
                || x?.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer
                 && x.PublicInterestCheck.SecondaryCheckUserId != userDto.Id)).ToList();

                publicInterestCheckViewModel.SecondaryChecksList = publicinterestchecks
                .Where(x => x.PublicInterestCheck !=null    && x.DaysLeftToComplete>0
                &&(x.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed ||
                x.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed)
                && x.PublicInterestCheck.PrimaryCheckUserId != userDto.Id).ToList();


                publicInterestCheckViewModel.ArchiveList = publicinterestchecks
                .Where(x => x.PublicInterestCheck != null &&
                (x.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckFailed ||
                 x.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckPassed)).ToList();

                return View(publicInterestCheckViewModel);
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }

        }
    }
}