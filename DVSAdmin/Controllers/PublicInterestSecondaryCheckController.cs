using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("public-interest-secondary-check")]
    public class PublicInterestSecondaryCheckController : Controller
    {
        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IUserService userService;
        private string UserEmail => HttpContext.Session.Get<string>("Email")??string.Empty;
        public PublicInterestSecondaryCheckController(IPublicInterestCheckService publicInterestCheckService, IUserService userService)
        {
            this.publicInterestCheckService = publicInterestCheckService;
            this.userService = userService;
        }


        #region Review
        [Route("public-interest-secondary-check-review")]
        public async Task<IActionResult> SecondaryCheckReview(int serviceId)
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = new();      
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
            return View("Review/SecondaryCheckReview", publicInterestSecondaryCheckViewModel);
        }


        [HttpPost("save-secondary-check-review")]
        public async Task<IActionResult> SaveSecondaryCheckReview(PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel, string saveReview)
        {
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(publicInterestSecondaryCheckViewModel.ServiceId);
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModelData = new ();
            secondaryCheckViewModelData = MapDtoToViewModel(serviceDto);
            secondaryCheckViewModelData.Service = serviceDto;
            secondaryCheckViewModelData.SecondaryCheckUserId = publicInterestSecondaryCheckViewModel.SecondaryCheckUserId;
            secondaryCheckViewModelData.SecondaryCheckComment = publicInterestSecondaryCheckViewModel.SecondaryCheckComment;
            AddModelErrorForInvalidActions(publicInterestSecondaryCheckViewModel, saveReview);
            HttpContext?.Session.Set("SecondaryCheckData", secondaryCheckViewModelData);

            if (ModelState.IsValid)
            {
                if (saveReview == "sentback")
                {
                    return RedirectToAction("ConfirmSentBackForPrimaryCheck", "PublicInterestSecondaryCheck");
                }
                else if (saveReview == "reject")
                {
                    return RedirectToAction("ProceedSecondaryCheckReject", "PublicInterestSecondaryCheck");
                }
                else if (saveReview == "approve")
                {
                    return RedirectToAction("ProceedSecondaryCheckApproval", "PublicInterestSecondaryCheck");
                }
                else
                {
                    HttpContext?.Session.Remove("SecondaryCheckData");
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                return View("Review/SecondaryCheckReview", secondaryCheckViewModelData);
            }

        }
#endregion

        #region Approve Flow

        [HttpGet("proceed-secondary-check-approve")]
        public async Task<IActionResult> ProceedSecondaryCheckApproval()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            ServiceDto service = await publicInterestCheckService.GetServiceDetailsWithMappings(publicInterestSecondaryCheckViewModel.ServiceId);
            return View("Approve/ProceedSecondaryCheckApproval", service);
        }

        [HttpPost("proceed-secondary-check-approve")]
        public IActionResult ProceedSecondaryCheckApproval(string saveReview)
        {
            if (saveReview == "approve")
            {
                return RedirectToAction("ConfirmSecondaryCheckApproval", "PublicInterestSecondaryCheck");
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("SecondaryCheckReview", "PublicInterestSecondaryCheck");
            }
            else
            {
                HttpContext?.Session.Remove("SecondaryCheckData");
                return RedirectToAction("HandleException", "Error");
            }

        }


        [HttpGet("secondary-check-approval")]
        public IActionResult ConfirmSecondaryCheckApproval()
        {
            return View("Approve/ConfirmSecondaryCheckApproval");
        }

        [HttpPost("secondary-check-approval")]
        public async Task<IActionResult> SaveSecondaryCheckApproval()
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            GenericResponse genericResponse;
            if (secondaryCheckViewModel != null && secondaryCheckViewModel.ServiceId > 0 && secondaryCheckViewModel.ProviderProfileId>0)
            {
                secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckPassed;
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
                genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
                if (genericResponse.Success)
                {
                    return RedirectToAction("SecondaryCheckApprovalConfirmation", "PublicInterestSecondaryCheck");
                }
                else
                {
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }


        }

        [HttpGet("secondary-check-approval-confirmation")]
        public IActionResult SecondaryCheckApprovalConfirmation()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");         
            HttpContext.Session.Remove("SecondaryCheckData");
            return View("Approve/SecondaryCheckApprovalConfirmation", publicInterestSecondaryCheckViewModel.Service);
        }
        #endregion


        #region Reject Flow

        [HttpGet("proceed-secondary-check-reject")]
        public IActionResult ProceedSecondaryCheckReject()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            if(publicInterestSecondaryCheckViewModel?.SelectedReasons != null && publicInterestSecondaryCheckViewModel?.SelectedReasons.Count>0)
            {
                publicInterestSecondaryCheckViewModel.SelectedReasonIds = publicInterestSecondaryCheckViewModel?.SelectedReasons?.Select(c => c.Id).ToList();
            }
            else
            {
                publicInterestSecondaryCheckViewModel.SelectedReasonIds = new List<int>();
            }
                   

            return View("Reject/ProceedSecondaryCheckReject", publicInterestSecondaryCheckViewModel);
        }

        [HttpPost("proceed-secondary-check-reject")]
        public IActionResult ProceedSecondaryCheckReject(PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel, string saveReview)
        {

            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            secondaryCheckViewModel.SelectedReasonIds =  secondaryCheckViewModel.SelectedReasonIds??new List<int>();
           
            if (saveReview == "reject")
            {
                secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckFailed;
                AddModelStateErrorIfRejectionReasonNull(publicInterestSecondaryCheckViewModel);

                if (ModelState.IsValid)
                {                    
                    secondaryCheckViewModel.SelectedReasons = publicInterestSecondaryCheckViewModel.AvailableReasons.Where(c => publicInterestSecondaryCheckViewModel.SelectedReasonIds.Contains(c.Id)).ToList();
                    HttpContext?.Session.Set("SecondaryCheckData", secondaryCheckViewModel);
                    return RedirectToAction("ConfirmSecondaryCheckReject", "PublicInterestSecondaryCheck");
                }
                else
                {
                    return View("Reject/ProceedSecondaryCheckReject", secondaryCheckViewModel);
                }
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("SecondaryCheckReview", "PublicInterestSecondaryCheck");
            }
            else
            {
                HttpContext.Session.Remove("SecondaryCheckData");
                return RedirectToAction("HandleException", "Error");
            }

        }



        /// <summary>
        /// About to reject
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-secondary-check-reject")]
        public IActionResult ConfirmSecondaryCheckReject()
        {
            return View("Reject/ConfirmSecondaryCheckReject");
        }

        /// <summary>
        /// Save Appplication Rejection
        /// </summary>
        /// <returns></returns>

        [HttpPost("save-rejection")]
        public async Task<IActionResult> SaveApplicationRejection()
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            if (secondaryCheckViewModel != null && secondaryCheckViewModel.ServiceId > 0 && secondaryCheckViewModel.ProviderProfileId>0)
            {
                secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckFailed;
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
                GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
                if (genericResponse.Success)
                {
                    return RedirectToAction("SecondaryCheckRejectionConfirmation", "PublicInterestSecondaryCheck");
                }
                else
                {
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }

        }


        /// <summary>
        /// Final page after rejection
        /// </summary>
        /// <returns></returns>
        [HttpGet("application-rejection-confirmation")]
        public IActionResult SecondaryCheckRejectionConfirmation()
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            HttpContext.Session.Remove("SecondaryCheckData");
            return View("Reject/SecondaryCheckRejectionConfirmation", secondaryCheckViewModel.Service);
        }
        #endregion

        #region Disagree /Sent Back to primary

        /// <summary>
        /// Intermediate screen for confirming sent back
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-sent-back")]
        public IActionResult ConfirmSentBackForPrimaryCheck()
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            return View("Disagree/ConfirmSentBackForPrimaryCheck", secondaryCheckViewModel);
        }


        [HttpPost("save-sent-back")]
        public async Task<IActionResult> SaveSentBackForPrimaryCheck(string saveReview)
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");

            if (secondaryCheckViewModel != null && secondaryCheckViewModel.ServiceId > 0 && secondaryCheckViewModel.ProviderProfileId>0)
            {
                if (saveReview == "sentback")
                {
                    secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.SentBackBySecondReviewer;
                    PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
                    GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
                    if (genericResponse.Success)
                    {
                        return RedirectToAction("SentBackConfirmation", "PublicInterestSecondaryCheck");
                    }
                    else
                    {
                        return RedirectToAction("HandleException", "Error");
                    }
                }
                else if (saveReview == "cancel")
                {
                    return RedirectToAction("SecondaryCheckReview", "PublicInterestSecondaryCheck");
                }
                else
                {
                    HttpContext.Session.Remove("SecondaryCheckData");
                    return RedirectToAction("HandleException", "Error");
                }

            }
            else
            {
                HttpContext.Session.Remove("SecondaryCheckData");
                return RedirectToAction("HandleException", "Error");
            }


        }

        /// <summary>
        /// Final screen for sent back flow
        /// </summary>
        /// <returns></returns>

        [HttpGet("sent-back-confirmation")]
        public IActionResult SentBackConfirmation()
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            HttpContext.Session.Remove("SecondaryCheckData");
            return View("Disagree/SentBackConfirmation", secondaryCheckViewModel.Service);
        }
        #endregion


        #region private methods
        private PublicInterestSecondaryCheckViewModel GetSecondaryCheckDataFromSession(HttpContext context, string key)
        {
            PublicInterestSecondaryCheckViewModel model = context?.Session.Get<PublicInterestSecondaryCheckViewModel>(key)??new PublicInterestSecondaryCheckViewModel();
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

        private PublicInterestCheckDto MapViewModelToDto(PublicInterestSecondaryCheckViewModel secondaryCheckViewModel)
        {
            PublicInterestCheckDto publicInterestCheckDto = new();
            publicInterestCheckDto.ServiceId =secondaryCheckViewModel.ServiceId;
            publicInterestCheckDto.ProviderProfileId = secondaryCheckViewModel.ProviderProfileId;
            publicInterestCheckDto.PublicInterestCheckStatus = secondaryCheckViewModel.PublicInterestCheckStatus;
            if(secondaryCheckViewModel.SelectedReasons!=null)
            {
                publicInterestCheckDto.RejectionReasons = string.Join('~', secondaryCheckViewModel.SelectedReasons.Select(r => r.RejectionReasonName));
            }          
            publicInterestCheckDto.SecondaryCheckUserId = secondaryCheckViewModel.SecondaryCheckUserId;
            publicInterestCheckDto.SecondaryCheckComment = secondaryCheckViewModel.SecondaryCheckComment;
            return publicInterestCheckDto;
        }


        private void AddModelErrorForInvalidActions(PublicInterestSecondaryCheckViewModel secondaryCheckViewModel, string reviewAction)
        {
            if (secondaryCheckViewModel.PrimaryCheckUserId == secondaryCheckViewModel.SecondaryCheckUserId)
            {
                ModelState.AddModelError("SubmitValidation", "Primary and secondary check user should be different");
            }
            else if (string.IsNullOrEmpty(secondaryCheckViewModel.SecondaryCheckComment))
            {
                ModelState.AddModelError("Comment", "Enter a comment to explain the checks completed");
            }

        }

        private void AddModelStateErrorIfRejectionReasonNull(PublicInterestSecondaryCheckViewModel secondaryCheckViewModel)
        {
            if (secondaryCheckViewModel.SelectedReasonIds == null || secondaryCheckViewModel.SelectedReasonIds.Count == 0)
                ModelState.AddModelError("SelectedReasonIds", "Select a reason for rejection");
        }
        #endregion
    }
}
