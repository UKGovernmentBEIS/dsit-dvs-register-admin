using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{

    [Route("public-interest-check")]
    
    public class PublicInterestCheckController : BaseController
    {
        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;   

        public PublicInterestCheckController(IPublicInterestCheckService publicInterestCheckService, IUserService userService, IConfiguration configuration)
        {
            this.publicInterestCheckService = publicInterestCheckService;
            this.userService = userService;
            this.configuration = configuration;
        }

        [HttpGet("public-interest-check-list")]
        public async Task<IActionResult> PublicInterestCheck()
        {
           
            if (string.IsNullOrEmpty(UserEmail))
                throw new InvalidOperationException("User email is missing.");

            
            UserDto userDto = await userService.GetUser(UserEmail);
            PublicInterestCheckViewModel publicInterestCheckViewModel = new PublicInterestCheckViewModel();

            var publicinterestchecks = await publicInterestCheckService.GetPICheckList();

            publicInterestCheckViewModel.PrimaryChecksList = publicinterestchecks.
            Where(x => (x.ServiceStatus == ServiceStatusEnum.Received && x.ServiceStatus != ServiceStatusEnum.Removed 
            && x.ServiceStatus!=ServiceStatusEnum.SavedAsDraft  &&
            x.Id != x?.PublicInterestCheck?.ServiceId ) ||
            ( x?.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer)
             && x.PublicInterestCheck.SecondaryCheckUserId != userDto.Id).OrderBy(x => x.DaysLeftToCompletePICheck).ToList();

            publicInterestCheckViewModel.SecondaryChecksList = publicinterestchecks
            .Where(x => x.ServiceStatus != ServiceStatusEnum.Removed
            &&  x.ServiceStatus != ServiceStatusEnum.SavedAsDraft && x.PublicInterestCheck !=null   
            &&(x.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed ||
            x.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed)
            && x.PublicInterestCheck.PrimaryCheckUserId != userDto.Id).OrderBy(x => x.DaysLeftToCompletePICheck).ToList();


            publicInterestCheckViewModel.ArchiveList = publicinterestchecks
            .Where(x => x.PublicInterestCheck != null && x.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
            (x.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckFailed ||
             x.PublicInterestCheck?.PublicInterestCheckStatus == PublicInterestCheckEnum.PublicInterestCheckPassed)).OrderByDescending(x => x.PublicInterestCheck.SecondaryCheckTime).ToList();

            return View(publicInterestCheckViewModel);

        }


       
        [HttpGet("archive-details")]
        public async Task<IActionResult> ArchiveDetails(int serviceId)
        {
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);       
            return View(serviceDto);

        }

      
    }
}