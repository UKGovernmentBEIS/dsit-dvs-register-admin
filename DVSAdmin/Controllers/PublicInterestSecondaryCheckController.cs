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

            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);
            if (serviceDto.ServiceStatus == ServiceStatusEnum.Received && ( serviceDto.PublicInterestCheck !=null 
                && (serviceDto.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckPassed 
                || serviceDto.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed 
                || serviceDto.PublicInterestCheck.PublicInterestCheckStatus == PublicInterestCheckEnum.SentBackBySecondReviewer)))
                return View("Review/SecondaryCheckReview", serviceDto);
            else return NotFound();
        }

        
        #endregion

        #region Approve Flow

        [HttpGet("proceed-secondary-check-approve")]
        public async Task<IActionResult> ProceedSecondaryCheckApproval(int serviceId)
        {         
            ServiceDto service = await publicInterestCheckService.GetServiceDetailsForPublishing(serviceId);
            return View("Approve/ProceedSecondaryCheckApproval", service);
        }



        [HttpGet("publish-service")]
        public async Task<IActionResult> PublishService(int serviceId)
        {
            
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetailsForPublishing(serviceId);
            if (serviceDto == null)
                return NotFound();
            ViewData["HidePublishStatus"] = true;
            ViewData["isPiCheck"] = true;
            return View("Approve/PublishService", serviceDto);
        }

        [HttpPost("publish-service")]
        public async Task<IActionResult> SavePublishService(int serviceId , string action)
        {
            if (action == "proceed")
            {
                ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetailsForPublishing(serviceId);
                UserDto userDto = await userService.GetUser(UserEmail);

                if (userDto.Id <= 0)
                    throw new InvalidOperationException("User is not valid.");

                PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = new();
                GenericResponse genericResponse;              

                secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.PublicInterestCheckPassed;
                secondaryCheckViewModel.ServiceId = serviceId;
                secondaryCheckViewModel.ProviderProfileId = serviceDto.Provider.Id;
                secondaryCheckViewModel.SecondaryCheckUserId = userDto.Id;
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(secondaryCheckViewModel);
                genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.SecondaryCheck, UserEmail);
                if (genericResponse.Success)
                {                  
                    return RedirectToAction("ServicePublishedConfirmation", "PublicInterestSecondaryCheck", new { serviceId  = serviceId });
                }
                  
                else
                    throw new InvalidOperationException("Failed to save secondary check approval.");

            }
            else if (action == "cancel")
            {
                return RedirectToAction("ProceedSecondaryCheckApproval", "PublicInterestSecondaryCheck");
            }
            else
            {
                HttpContext?.Session.Remove("SecondaryCheckData");
                throw new InvalidOperationException("Invalid action during secondary check approval.");
            }
        }

        [HttpGet("service-published-confirmation")]
        public async Task<IActionResult> ServicePublishedConfirmation(int serviceId)
        {
          
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetailsWithMappings(serviceId);
            HttpContext.Session.Remove("SecondaryCheckData");
            return View("Approve/ServicePublishedConfirmation", serviceDto);
        }
        #endregion

        #region Reject Flow

        [HttpGet("proceed-secondary-check-reject")]
        public async Task <IActionResult> ProceedSecondaryCheckReject(int serviceId)
        {
            PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            if(serviceId!=0)
            {
                ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);
                publicInterestSecondaryCheckViewModel.Service = serviceDto;
                publicInterestSecondaryCheckViewModel.ServiceId = serviceDto.Id;
                publicInterestSecondaryCheckViewModel.ProviderProfileId = serviceDto.Provider.Id;
            }
           
           
            if (publicInterestSecondaryCheckViewModel?.SelectedReasons != null && publicInterestSecondaryCheckViewModel?.SelectedReasons.Count>0)
                publicInterestSecondaryCheckViewModel.SelectedReasonIds = publicInterestSecondaryCheckViewModel?.SelectedReasons?.Select(c => c.Id).ToList();
            else
                publicInterestSecondaryCheckViewModel.SelectedReasonIds = [];

            return View("Reject/ProceedSecondaryCheckReject", publicInterestSecondaryCheckViewModel);
        }

        [HttpPost("proceed-secondary-check-reject")]
        public async Task<IActionResult> ProceedSecondaryCheckReject(PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel)
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            secondaryCheckViewModel.SelectedReasonIds = publicInterestSecondaryCheckViewModel.SelectedReasonIds??[];
            secondaryCheckViewModel.ServiceId = publicInterestSecondaryCheckViewModel.ServiceId;
            secondaryCheckViewModel.ProviderProfileId = publicInterestSecondaryCheckViewModel.ProviderProfileId;
            secondaryCheckViewModel.Service = await publicInterestCheckService.GetServiceDetails(publicInterestSecondaryCheckViewModel.ServiceId);
            if (ModelState["SelectedReasonIds"]?.Errors.Count == 0)
            {
                UserDto userDto = await userService.GetUser(UserEmail);

                if (userDto.Id <= 0)
                    throw new InvalidOperationException("User is not valid.");
                secondaryCheckViewModel.SecondaryCheckUserId = userDto.Id;
                secondaryCheckViewModel.SelectedReasons = publicInterestSecondaryCheckViewModel.AvailableReasons.Where(c => publicInterestSecondaryCheckViewModel.SelectedReasonIds.Contains(c.Id)).ToList();
                HttpContext?.Session.Set("SecondaryCheckData", secondaryCheckViewModel);
                return RedirectToAction("ConfirmSecondaryCheckReject", "PublicInterestSecondaryCheck");
            }
            else
            {

                return View("Reject/ProceedSecondaryCheckReject", secondaryCheckViewModel);
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
        public async Task<IActionResult> ConfirmSentBackForPrimaryCheck(int serviceId)
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            if (serviceId != 0)
            {
                ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);
                secondaryCheckViewModel.Service = serviceDto;
                secondaryCheckViewModel.ServiceId = serviceDto.Id;
                secondaryCheckViewModel.ProviderProfileId = serviceDto.Provider.Id;
            }

            return View("Disagree/ConfirmSentBackForPrimaryCheck", secondaryCheckViewModel);
        }


        [HttpPost("save-sent-back")]
        public async Task<IActionResult> SaveSentBackForPrimaryCheck(PublicInterestSecondaryCheckViewModel publicInterestSecondaryCheckViewModel)
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            secondaryCheckViewModel.SecondaryCheckComment = publicInterestSecondaryCheckViewModel.SecondaryCheckComment;
            secondaryCheckViewModel.ServiceId = publicInterestSecondaryCheckViewModel.ServiceId;
            secondaryCheckViewModel.ProviderProfileId = publicInterestSecondaryCheckViewModel.ProviderProfileId;
            secondaryCheckViewModel.Service = await publicInterestCheckService.GetServiceDetails(publicInterestSecondaryCheckViewModel.ServiceId);

            UserDto userDto = await userService.GetUser(UserEmail);

            if (userDto.Id <= 0)
                throw new InvalidOperationException("User is not valid.");

            secondaryCheckViewModel.PrimaryCheckUserId = userDto.Id;
            secondaryCheckViewModel.SecondaryCheckUserId = secondaryCheckViewModel.Service.PublicInterestCheck.SecondaryCheckUserId;
            AddModelErrorForInvalidActions(publicInterestSecondaryCheckViewModel);
            if (secondaryCheckViewModel == null || secondaryCheckViewModel.ServiceId <= 0 || secondaryCheckViewModel.ProviderProfileId <= 0)
            {
                HttpContext.Session.Remove("SecondaryCheckData");
                throw new InvalidOperationException("Secondary check session data is missing or invalid.");
            }

            if (ModelState["SecondaryCheckComment"]?.Errors.Count == 0)
            {
               
                secondaryCheckViewModel.PublicInterestCheckStatus = PublicInterestCheckEnum.SentBackBySecondReviewer;
                secondaryCheckViewModel.SecondaryCheckUserId = userDto.Id;
                HttpContext?.Session.Set("SecondaryCheckData", secondaryCheckViewModel);
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
            else
            {
                return View("Disagree/ConfirmSentBackForPrimaryCheck", secondaryCheckViewModel);
            }
        }

        /// <summary>
        /// Final screen for sent back flow
        /// </summary>
        /// <returns></returns>

        [HttpGet("sent-back-confirmation")]
        public async Task<IActionResult> SentBackConfirmation()
        {
            PublicInterestSecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(secondaryCheckViewModel.ServiceId);
            HttpContext.Session.Remove("SecondaryCheckData");
            return View("Disagree/SentBackConfirmation", serviceDto);
        }
        #endregion

        #region private methods
        private PublicInterestSecondaryCheckViewModel GetSecondaryCheckDataFromSession(HttpContext context, string key)
        {
            PublicInterestSecondaryCheckViewModel model = context?.Session.Get<PublicInterestSecondaryCheckViewModel>(key)??new PublicInterestSecondaryCheckViewModel();
            return model;
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


        private void AddModelErrorForInvalidActions(PublicInterestSecondaryCheckViewModel secondaryCheckViewModel)
        {
            if (secondaryCheckViewModel.PrimaryCheckUserId == secondaryCheckViewModel.SecondaryCheckUserId)
            {
                ModelState.AddModelError("SubmitValidation", "Primary and secondary check user should be different");
            }
          

        }

       
        #endregion
    }
}
