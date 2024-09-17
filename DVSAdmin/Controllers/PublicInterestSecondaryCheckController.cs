using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("public-interest-secondary-check")]
    public class PublicInterestSecondaryCheckController : Controller
    {
        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IUserService userService;
        public PublicInterestSecondaryCheckController(IPublicInterestCheckService publicInterestCheckService, IUserService userService)
        {
            this.publicInterestCheckService = publicInterestCheckService;
            this.userService = userService;
        }

        [Route("public-interest-secondary-check-review")]
        public async Task<IActionResult> PublicInterestSecondaryCheck(int serviceId)
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = new PublicInterestSecondaryCheckViewModel();
            if (serviceId == 0)
            {
                publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            }
            else
            {
                string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
                if (!string.IsNullOrEmpty(loggedinUserEmail))
                {
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);
                    if (userDto.Id > 0)
                    {

                        ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);
                        publicInterestSecondaryCheckViewModel = MapDtoToViewModel(serviceDto);
                        publicInterestSecondaryCheckViewModel.SecondaryCheckUserId = userDto.Id;
                    }
                    else
                    {
                        return RedirectToAction(Constants.ErrorPath);
                    }

                }
                else
                {
                    return RedirectToAction(Constants.ErrorPath);
                }
            }
            return View(publicInterestSecondaryCheckViewModel);
        }


        [HttpPost("save-secondary-check-review")]
        public async Task<IActionResult> SaveSecondaryCheckReview(PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel, string saveReview)
        {
            //to do implementation
            return View(publicInterestSecondaryCheckViewModel);

        }


        #region private methods
        private PublicInterestSecondaryCheckViewModel GetSecondaryCheckDataFromSession(HttpContext context, string key)
        {
            PublicInterestSecondaryCheckViewModel model = context?.Session.Get<PublicInterestSecondaryCheckViewModel>(key);
            return model;
        }

        private PublicInterestSecondaryCheckViewModel MapDtoToViewModel(ServiceDto serviceDto)
        {

            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = new()
            {
                Service = serviceDto,
                ServiceId = serviceDto.Id,
                ProviderProfileId = serviceDto.Provider.Id,
            };
            if (serviceDto.PublicInterestCheck != null)
            {
                publicInterestSecondaryCheckViewModel.IsCompanyHouseNumberApproved = serviceDto.PublicInterestCheck.IsCompanyHouseNumberApproved;
                publicInterestSecondaryCheckViewModel.IsDirectorshipsApproved = serviceDto.PublicInterestCheck.IsDirectorshipsApproved;
                publicInterestSecondaryCheckViewModel.IsDirectorshipsAndRelationApproved = serviceDto.PublicInterestCheck.IsDirectorshipsAndRelationApproved;
                publicInterestSecondaryCheckViewModel.IsTradingAddressApproved = serviceDto.PublicInterestCheck.IsTradingAddressApproved;
                publicInterestSecondaryCheckViewModel.IsSanctionListApproved = serviceDto.PublicInterestCheck.IsSanctionListApproved;
                publicInterestSecondaryCheckViewModel.IsUNFCApproved = serviceDto.PublicInterestCheck.IsUNFCApproved;
                publicInterestSecondaryCheckViewModel.IsECCheckApproved = serviceDto.PublicInterestCheck.IsECCheckApproved;
                publicInterestSecondaryCheckViewModel.IsTARICApproved = serviceDto.PublicInterestCheck.IsTARICApproved;
                publicInterestSecondaryCheckViewModel.IsBannedPoliticalApproved = serviceDto.PublicInterestCheck.IsBannedPoliticalApproved;
                publicInterestSecondaryCheckViewModel.IsProvidersWebpageApproved = serviceDto.PublicInterestCheck.IsProvidersWebpageApproved;
                publicInterestSecondaryCheckViewModel.PrimaryCheckComment = serviceDto.PublicInterestCheck.PrimaryCheckComment;
                publicInterestSecondaryCheckViewModel.PublicInterestCheckStatus = serviceDto.PublicInterestCheck.PublicInterestCheckStatus;
            }
            return publicInterestSecondaryCheckViewModel;
        }
        #endregion
    }
}
