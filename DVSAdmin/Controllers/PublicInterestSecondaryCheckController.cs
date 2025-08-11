using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;


namespace DVSAdmin.Controllers
{
   
    [Route("public-interest-secondary-check")]
    public class PublicInterestSecondaryCheckController : BaseController
    {
        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IUserService userService;      
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
                
                if (string.IsNullOrEmpty(UserEmail))
                    throw new InvalidOperationException("User email is missing.");
                
                UserDto userDto = await userService.GetUser(UserEmail);

                if (userDto.Id <= 0)
                    throw new InvalidOperationException("User is not valid.");

                ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);
                
                if (serviceDto.ServiceStatus == ServiceStatusEnum.Removed || serviceDto.ServiceStatus == ServiceStatusEnum.SavedAsDraft)
                    throw new InvalidOperationException("Service is not in a valid status for secondary check review.");

                publicInterestSecondaryCheckViewModel = MapDtoToViewModel(serviceDto);
                publicInterestSecondaryCheckViewModel.SecondaryCheckUserId = userDto.Id;
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
                return saveReview switch
                {
                    "sentback" => RedirectToAction("ConfirmSentBackForPrimaryCheck", "PublicInterestSecondaryCheck"),
                    "reject" => RedirectToAction("ProceedSecondaryCheckReject", "PublicInterestSecondaryCheck"),
                    "approve" => RedirectToAction("ProceedSecondaryCheckApproval", "PublicInterestSecondaryCheck"),
                    _ => throw new InvalidOperationException("Unexpected review action in secondary check."),
                };
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
        public async Task<IActionResult> ProceedSecondaryCheckApproval(string saveReview)
        {
            if (saveReview == "approve")
            {
                PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
                GenericResponse genericResponse;

                if (secondaryCheckViewModel == null || secondaryCheckViewModel.ServiceId <= 0 || secondaryCheckViewModel.ProviderProfileId <= 0)
                    throw new InvalidOperationException("Secondary check session data is missing or invalid.");

                secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckPassed;
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
                genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
                if (genericResponse.Success)
                    return RedirectToAction("PublishService", "PublicInterestSecondaryCheck");
                else
                    throw new InvalidOperationException("Failed to save secondary check approval.");

               
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("SecondaryCheckReview", "PublicInterestSecondaryCheck");
            }
            else
            {
                HttpContext?.Session.Remove("SecondaryCheckData");
                throw new InvalidOperationException("Invalid action during secondary check approval.");
            }

        }


        [HttpGet("publish-service")]
        public async Task<IActionResult> PublishService()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetailsForPublishing(publicInterestSecondaryCheckViewModel.ServiceId);
            return View("Approve/PublishService", serviceDto);
        }

        [HttpPost("publish-service")]
        public async Task<IActionResult> SavePublishService()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            GenericResponse genericResponse = await publicInterestCheckService
           .UpdateServiceStatus(publicInterestSecondaryCheckViewModel.ServiceId,
            publicInterestSecondaryCheckViewModel.Service.ServiceName, publicInterestSecondaryCheckViewModel.ProviderProfileId, UserEmail, publicInterestSecondaryCheckViewModel.Service.CabUser.CabEmail);
            if (genericResponse.Success)
                return RedirectToAction("ServicePublishedConfirmation");
            else
                throw new InvalidOperationException("Failed to publish service");
        }


        //[HttpGet("secondary-check-approval")]
        //public IActionResult ConfirmSecondaryCheckApproval()
        //{
        //    return View("Approve/ConfirmSecondaryCheckApproval");
        //}

        //[HttpPost("secondary-check-approval")]
        //public async Task<IActionResult> SaveSecondaryCheckApproval()
        //{
        //    PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
        //    GenericResponse genericResponse;

        //    if (secondaryCheckViewModel == null || secondaryCheckViewModel.ServiceId <= 0 || secondaryCheckViewModel.ProviderProfileId <= 0)
        //        throw new InvalidOperationException("Secondary check session data is missing or invalid.");

        //    secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckPassed;
        //    PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
        //    genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
        //    if (genericResponse.Success)
        //        return RedirectToAction("SecondaryCheckApprovalConfirmation", "PublicInterestSecondaryCheck");
        //    else
        //        throw new InvalidOperationException("Failed to save secondary check approval.");
        //}






        [HttpGet("service-published-confirmation")]
        public async Task<IActionResult> ServicePublishedConfirmation()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetailsWithMappings(publicInterestSecondaryCheckViewModel.ServiceId);
            HttpContext.Session.Remove("SecondaryCheckData");
            return View("Approve/ServicePublishedConfirmation", serviceDto);
        }
        #endregion

        #region Reject Flow

        [HttpGet("proceed-secondary-check-reject")]
        public IActionResult ProceedSecondaryCheckReject()
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            if(publicInterestSecondaryCheckViewModel?.SelectedReasons != null && publicInterestSecondaryCheckViewModel?.SelectedReasons.Count>0)
                publicInterestSecondaryCheckViewModel.SelectedReasonIds = publicInterestSecondaryCheckViewModel?.SelectedReasons?.Select(c => c.Id).ToList();
            else
                publicInterestSecondaryCheckViewModel.SelectedReasonIds = new List<int>();

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
                throw new InvalidOperationException("Invalid action during secondary check rejection.");
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
            
            if (secondaryCheckViewModel == null || secondaryCheckViewModel.ServiceId <= 0 || secondaryCheckViewModel.ProviderProfileId <= 0)
                throw new InvalidOperationException("Secondary check session data is missing or invalid.");
            
            secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckFailed;
            PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
            GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
            if (genericResponse.Success)
                return RedirectToAction("SecondaryCheckRejectionConfirmation", "PublicInterestSecondaryCheck");
            else
                throw new InvalidOperationException("Failed to save secondary check rejection.");
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

            if (secondaryCheckViewModel == null || secondaryCheckViewModel.ServiceId <= 0 || secondaryCheckViewModel.ProviderProfileId <= 0)
            {
                HttpContext.Session.Remove("SecondaryCheckData");
                throw new InvalidOperationException("Secondary check session data is missing or invalid.");
            }   
            
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
                    throw new InvalidOperationException("Failed to save sent back status.");
                }
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("SecondaryCheckReview", "PublicInterestSecondaryCheck");
            }
            else
            {
                HttpContext.Session.Remove("SecondaryCheckData");
                throw new InvalidOperationException("Invalid action during secondary check sent back flow.");
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
