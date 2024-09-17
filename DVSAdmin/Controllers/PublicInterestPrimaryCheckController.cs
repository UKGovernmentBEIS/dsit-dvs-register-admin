using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;


namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("public-interest-primary-check")]
    public class PublicInterestPrimaryCheckController : Controller
    {

        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IUserService userService;
        public PublicInterestPrimaryCheckController(IPublicInterestCheckService publicInterestCheckService, IUserService userService)
        {
            this.publicInterestCheckService = publicInterestCheckService;
            this.userService = userService;
        }

        #region Review screen
        /// <summary>
        /// Review screen with approve/reject sections
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns></returns>
        [HttpGet("application-reivew")]
        public async Task<IActionResult> PrimaryCheckReview(int serviceId)
        {
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel;

            if (serviceId == 0)
            {
                publicInterestPrimaryCheckViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            }
            else
            {
                string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
                if (!string.IsNullOrEmpty(loggedinUserEmail))
                {
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);

                    if (userDto.Id>0)
                    {
                        ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);
                        publicInterestPrimaryCheckViewModel = MapDtoToViewModel(serviceDto);
                        publicInterestPrimaryCheckViewModel.PrimaryCheckUserId = userDto.Id;
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
            return View(publicInterestPrimaryCheckViewModel);

        }



        /// <summary>
        ///  On click of Saveas draft/ Pass/ Fail button click
        /// redirect to respective pages or save as draft
        /// </summary>
        /// <param name="publicInterestPrimaryCheckViewModel"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-primary-check-review")]
        public async Task<IActionResult> SavePrimaryCheckReview(PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel, string saveReview)
        {
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(publicInterestPrimaryCheckViewModel.ServiceId);
            publicInterestPrimaryCheckViewModel.Service = serviceDto;
            PublicInterestCheckEnum reviewStatus = GetApplicationStatus(publicInterestPrimaryCheckViewModel, saveReview);
            // To handle back ward navigation between screens and cancel
            HttpContext?.Session.Set("PrimaryCheckData", publicInterestPrimaryCheckViewModel);
            AddModelErrorForInvalidActions(publicInterestPrimaryCheckViewModel, saveReview);

            if (ModelState.IsValid)
            {
                publicInterestPrimaryCheckViewModel.PublicInterestCheckStatus = reviewStatus;
                publicInterestPrimaryCheckViewModel.PrimaryCheckComment = InputSanitizeExtensions.CleanseInput(publicInterestPrimaryCheckViewModel.PrimaryCheckComment??string.Empty);
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(publicInterestPrimaryCheckViewModel);
                if (reviewStatus == PublicInterestCheckEnum.InPrimaryReview)
                {
                    GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.PrimaryCheck);
                    if (genericResponse.Success)
                    {
                        return RedirectToAction("PrimaryCheckReview", new { serviceId = publicInterestPrimaryCheckViewModel.Service.Id });
                    }
                    else
                    { 
                        return RedirectToAction("HandleException", "Error");
                    }
                  
                }
                else if (reviewStatus == PublicInterestCheckEnum.PrimaryCheckPassed)
                {
                    return RedirectToAction("ConfirmPrimaryCheckPass", "PublicInterestPrimaryCheck");
                }
                else if (reviewStatus == PublicInterestCheckEnum.PrimaryCheckFailed)
                {
                    return RedirectToAction("ConfirmPrimaryCheckFail", "PublicInterestPrimaryCheck");
                }
                else
                {
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                HttpContext?.Session.Remove("PrimaryCheckData");
                return View("PrimaryCheckReview", publicInterestPrimaryCheckViewModel);
            }

        }

        #endregion

        #region Pass primary check
        /// <summary>
        /// Intermediate page to save approval
        /// </summary>
        /// <returns></returns>
        [HttpGet("primary-check-approval")]
        public IActionResult ConfirmPrimaryCheckPass()
        {
            return View();
        }

        /// <summary>
        /// Save When Pass Primary check clicked in ConfirmPrimaryCheckPass page
        /// </summary>        
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-primary-check-passed")]
        public async Task<IActionResult> SavePrimaryCheckPassed(string saveReview)
        {

            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            if (publicInterestPrimaryCheckView != null)
            {

                if (saveReview == "save")
                {
                    if (publicInterestPrimaryCheckView != null && publicInterestPrimaryCheckView.ServiceId > 0)
                    {
                        PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(publicInterestPrimaryCheckView);
                        publicInterestCheckDto.PublicInterestCheckStatus = PublicInterestCheckEnum.PrimaryCheckPassed;
                        GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.PrimaryCheck);
                        if (genericResponse.Success)
                        {
                            return RedirectToAction("PrimaryCheckPassedConfirmation", "PublicInterestPrimaryCheck");
                        }
                        else
                        {
                            HttpContext.Session.Remove("PrimaryCheckData");
                            return RedirectToAction("HandleException", "Error");
                        }
                    }
                    else
                    {
                        HttpContext.Session.Remove("PrimaryCheckData");
                        return RedirectToAction("HandleException", "Error");
                    }
                }
                else if (saveReview == "cancel") // on cancel click go back to previous page with curent data
                {
                    return RedirectToAction("PrimaryCheckReview", "PublicInterestPrimaryCheck");
                }
                else
                {
                    HttpContext.Session.Remove("PrimaryCheckData");
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                HttpContext.Session.Remove("PrimaryCheckData");
                return RedirectToAction("HandleException", "Error");
            }

        }


        [HttpGet("primary-check-approval-confirmation")]
        public IActionResult PrimaryCheckPassedConfirmation()
        {
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            HttpContext.Session.Remove("PrimaryCheckData");
            return View(publicInterestPrimaryCheckView.Service);
        }
        #endregion

        #region Private Methods

        private static PublicInterestPrimaryCheckViewModel GetPrimaryCheckDataFromSession(HttpContext context, string key)
        {
            PublicInterestPrimaryCheckViewModel model = context?.Session.Get<PublicInterestPrimaryCheckViewModel>(key)??new();
            return model;
        }
        private static  PublicInterestPrimaryCheckViewModel MapDtoToViewModel(ServiceDto serviceDto)
        {

            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel = new()
            {
                Service = serviceDto,
                ServiceId = serviceDto.Id,
                ProviderProfileId = serviceDto.Provider.Id,
             };
            if (serviceDto.PublicInterestCheck!= null)
            {                
                publicInterestPrimaryCheckViewModel.IsCompanyHouseNumberApproved= serviceDto.PublicInterestCheck.IsCompanyHouseNumberApproved;
                publicInterestPrimaryCheckViewModel.IsDirectorshipsApproved = serviceDto.PublicInterestCheck.IsDirectorshipsApproved;
                publicInterestPrimaryCheckViewModel.IsDirectorshipsAndRelationApproved= serviceDto.PublicInterestCheck.IsDirectorshipsAndRelationApproved;
                publicInterestPrimaryCheckViewModel.IsTradingAddressApproved= serviceDto.PublicInterestCheck.IsTradingAddressApproved;
                publicInterestPrimaryCheckViewModel.IsSanctionListApproved= serviceDto.PublicInterestCheck.IsSanctionListApproved;
                publicInterestPrimaryCheckViewModel.IsUNFCApproved= serviceDto.PublicInterestCheck.IsUNFCApproved;
                publicInterestPrimaryCheckViewModel.IsECCheckApproved= serviceDto.PublicInterestCheck.IsECCheckApproved;
                publicInterestPrimaryCheckViewModel.IsTARICApproved= serviceDto.PublicInterestCheck.IsTARICApproved;
                publicInterestPrimaryCheckViewModel.IsBannedPoliticalApproved= serviceDto.PublicInterestCheck.IsBannedPoliticalApproved;
                publicInterestPrimaryCheckViewModel.IsProvidersWebpageApproved= serviceDto.PublicInterestCheck.IsProvidersWebpageApproved;
                publicInterestPrimaryCheckViewModel.PrimaryCheckComment = serviceDto.PublicInterestCheck.PrimaryCheckComment;
                publicInterestPrimaryCheckViewModel.PublicInterestCheckStatus = serviceDto.PublicInterestCheck.PublicInterestCheckStatus;
            }
            return publicInterestPrimaryCheckViewModel;
        }

        private static PublicInterestCheckDto MapViewModelToDto(PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel)
        {
            PublicInterestCheckDto publicInterestCheckDto = new()
            {
                ServiceId =publicInterestPrimaryCheckViewModel.ServiceId,
                ProviderProfileId = publicInterestPrimaryCheckViewModel.ProviderProfileId,
                IsCompanyHouseNumberApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsCompanyHouseNumberApproved),
                IsDirectorshipsApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsDirectorshipsApproved),
                IsDirectorshipsAndRelationApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsDirectorshipsAndRelationApproved),
                IsTradingAddressApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsTradingAddressApproved),
                IsSanctionListApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsSanctionListApproved),
                IsUNFCApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsUNFCApproved),
                IsECCheckApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsECCheckApproved),
                IsTARICApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsTARICApproved),
                IsBannedPoliticalApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsBannedPoliticalApproved),
                IsProvidersWebpageApproved= Convert.ToBoolean(publicInterestPrimaryCheckViewModel?.IsProvidersWebpageApproved),
                PrimaryCheckComment = publicInterestPrimaryCheckViewModel?.PrimaryCheckComment,
                PublicInterestCheckStatus = publicInterestPrimaryCheckViewModel.PublicInterestCheckStatus,
                PrimaryCheckUserId = Convert.ToInt32(publicInterestPrimaryCheckViewModel.PrimaryCheckUserId)
            };


            return publicInterestCheckDto;
        }

        private static PublicInterestCheckEnum GetApplicationStatus(PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel, string saveReview)
        {
            PublicInterestCheckEnum publicInterestCheckStatus = publicInterestPrimaryCheckViewModel.PublicInterestCheckStatus;//Default value

            if (saveReview == "approve")
                publicInterestCheckStatus = PublicInterestCheckEnum.PrimaryCheckPassed;
            else if (saveReview == "reject")
                publicInterestCheckStatus = PublicInterestCheckEnum.PrimaryCheckFailed;
            else if (saveReview == "draft")
                publicInterestCheckStatus = PublicInterestCheckEnum.InPrimaryReview;
            return publicInterestCheckStatus;

        }

        private void AddModelErrorForInvalidActions(PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel, string reviewAction)
        {           

            if (publicInterestPrimaryCheckViewModel.PrimaryCheckUserId == publicInterestPrimaryCheckViewModel.SecondaryCheckUserId)
            {
                ModelState.AddModelError("SubmitValidation", "Primary and secondary check user should be different");
            }
          
            else
            {
                if (publicInterestPrimaryCheckViewModel.IsCompanyHouseNumberApproved !=null && publicInterestPrimaryCheckViewModel.IsDirectorshipsApproved!=null
                    && publicInterestPrimaryCheckViewModel.IsDirectorshipsAndRelationApproved!=null && publicInterestPrimaryCheckViewModel.IsTradingAddressApproved!=null
                    && publicInterestPrimaryCheckViewModel.IsSanctionListApproved!=null && publicInterestPrimaryCheckViewModel.IsUNFCApproved!= null
                    && publicInterestPrimaryCheckViewModel.IsECCheckApproved!=null && publicInterestPrimaryCheckViewModel.IsTARICApproved!=null
                    && publicInterestPrimaryCheckViewModel.IsBannedPoliticalApproved != null && publicInterestPrimaryCheckViewModel.IsProvidersWebpageApproved != null)
                {

                   bool isCompanyHouseNumberApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsCompanyHouseNumberApproved);
                   bool isDirectorshipsApproved =  Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsDirectorshipsApproved);
                   bool isDirectorshipsAndRelationApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsDirectorshipsAndRelationApproved);
                   bool isTradingAddressApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsTradingAddressApproved);
                   bool isSanctionListApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsSanctionListApproved);
                   bool isUNFCApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsUNFCApproved);
                   bool isECCheckApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsECCheckApproved);
                   bool isTARICApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsTARICApproved);
                   bool isBannedPoliticalApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsBannedPoliticalApproved);
                   bool isProvidersWebpageApproved = Convert.ToBoolean(publicInterestPrimaryCheckViewModel.IsProvidersWebpageApproved);



                    if (isCompanyHouseNumberApproved && isDirectorshipsApproved &&  isDirectorshipsAndRelationApproved &&   isTradingAddressApproved &&
                        isSanctionListApproved && isUNFCApproved &&   isECCheckApproved && isTARICApproved && isBannedPoliticalApproved && isProvidersWebpageApproved
                        && !string.IsNullOrEmpty(publicInterestPrimaryCheckViewModel.PrimaryCheckComment)  && reviewAction == "reject")
                    {
                        ModelState.AddModelError("SubmitValidation", "Your decision to pass or fail this primary check must match with the selections");
                    }
                    else if ((isCompanyHouseNumberApproved == false ||  isDirectorshipsApproved == false || isDirectorshipsAndRelationApproved == false ||
                     isTradingAddressApproved == false || isSanctionListApproved == false || isUNFCApproved == false ||  isECCheckApproved == false ||
                     isTARICApproved == false ||   isBannedPoliticalApproved == false ||    isProvidersWebpageApproved == false) && !string.IsNullOrEmpty(publicInterestPrimaryCheckViewModel.PrimaryCheckComment)
                     && reviewAction == "approve")
                    {
                        ModelState.AddModelError("SubmitValidation", "Your decision to pass or fail this primary check must match with the selections");
                    }
                }
            }
        }
       
        #endregion
    }
}
