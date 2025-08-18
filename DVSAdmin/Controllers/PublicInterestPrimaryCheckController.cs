using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;


namespace DVSAdmin.Controllers
{
  
    [Route("public-interest-primary-check")]
    public class PublicInterestPrimaryCheckController : BaseController
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
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");

            if (string.IsNullOrEmpty(UserEmail))
                throw new InvalidOperationException("User email is missing.");

            UserDto userDto = await userService.GetUser(UserEmail);

            if (userDto.Id <= 0)
                throw new InvalidOperationException("User is not valid.");

            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(serviceId);

            if (serviceDto.ServiceStatus == ServiceStatusEnum.Removed || serviceDto.ServiceStatus == ServiceStatusEnum.SavedAsDraft)
                throw new InvalidOperationException("Service is not in a valid status for primary check review.");

            publicInterestPrimaryCheckViewModel = MapDtoToViewModel(serviceDto);
            publicInterestPrimaryCheckViewModel.ServiceId = serviceDto.Id;
            publicInterestPrimaryCheckViewModel.ProviderProfileId = serviceDto.Provider.Id;
            publicInterestPrimaryCheckViewModel.PrimaryCheckUserId = userDto?.Id;
            publicInterestPrimaryCheckViewModel.SecondaryCheckUserId = serviceDto.PublicInterestCheck.SecondaryCheckUserId;
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
        public async Task<IActionResult> SavePrimaryCheckReview(PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel)
        {
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(publicInterestPrimaryCheckViewModel.ServiceId);
            publicInterestPrimaryCheckViewModel.Service = serviceDto;
          
            AddModelErrorForInvalidActions(publicInterestPrimaryCheckViewModel);
            if(ModelState["PublicInterestChecksMet"]?.Errors.Count == 0)
            {
                publicInterestPrimaryCheckViewModel.PrimaryCheckComment = serviceDto.PublicInterestCheck.PrimaryCheckComment;
                HttpContext?.Session.Set("PrimaryCheckData", publicInterestPrimaryCheckViewModel);
                if (publicInterestPrimaryCheckViewModel.PublicInterestChecksMet == true)
                {                   
                    return RedirectToAction("ConfirmPrimaryCheckPass");
                }
                else  
                {                   
                    return RedirectToAction("ConfirmPrimaryCheckFail");
                }
            }
            else
            {
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
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");           
            return View(publicInterestPrimaryCheckView);
        }

        /// <summary>
        /// Save When Pass Primary check clicked in ConfirmPrimaryCheckPass page
        /// </summary>        
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-primary-check-passed")]
        public async Task<IActionResult> SavePrimaryCheckPassed( string saveReview)
        {

            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            ViewBag.serviceId = publicInterestPrimaryCheckView.ServiceId;
            if (publicInterestPrimaryCheckView == null)
                throw new InvalidOperationException("Primary check session data is missing.");

            if (saveReview == "save")
            {
                publicInterestPrimaryCheckView.PublicInterestCheckStatus = PublicInterestCheckEnum.PrimaryCheckPassed;
                if (publicInterestPrimaryCheckView.ServiceId <= 0)
                {
                    HttpContext.Session.Remove("PrimaryCheckData");
                    throw new InvalidOperationException("Invalid service ID in primary check view.");
                }
                publicInterestPrimaryCheckView.PublicInterestCheckStatus = PublicInterestCheckEnum.PrimaryCheckPassed;           
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(publicInterestPrimaryCheckView);              
                GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.PrimaryCheck, UserEmail);
                
                if (genericResponse.Success)
                {
                    return RedirectToAction("PrimaryCheckPassedConfirmation", "PublicInterestPrimaryCheck");
                }
                else
                {
                    HttpContext.Session.Remove("PrimaryCheckData");
                    throw new InvalidOperationException("Failed to save primary check as passed.");
                }
            }
            else if (saveReview == "cancel") // on cancel click go back to previous page with curent data
            {
                return RedirectToAction("PrimaryCheckReview", "PublicInterestPrimaryCheck", new { serviceId  = publicInterestPrimaryCheckView.ServiceId });
            }
            else
            {
                HttpContext.Session.Remove("PrimaryCheckData");
                throw new InvalidOperationException("Invalid review action for primary check approval.");
            }
        }


        [HttpGet("primary-check-approval-confirmation")]
        public async  Task<IActionResult> PrimaryCheckPassedConfirmation()
        {
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(publicInterestPrimaryCheckView.ServiceId);
            HttpContext.Session.Remove("PrimaryCheckData");
            return View(serviceDto);
        }
        #endregion


        #region Fail primary check
        /// <summary>
        /// Intermediate screen for confirming primary check fail
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-primary-check-fail")]
        public IActionResult ConfirmPrimaryCheckFail()
        {
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            return View(publicInterestPrimaryCheckView);
        }


        /// <summary>
        /// Save When Fail Primary check clicked in ConfirmPrimaryCheckFail page
        /// On cancel redirect back to PrimaryCheckReview
        /// </summary>
        /// <param name="preRegistrationReviewViewModelstring"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-primary-check-failed")]
        public async Task<IActionResult> SavePrimaryCheckFailed(PublicInterestPrimaryCheckViewModel viewModel)
        {
            //Get data in previous page stored in session , ans save to database
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            if (ModelState["PrimaryCheckComment"]?.Errors.Count == 0)
            {

                if (publicInterestPrimaryCheckView == null || publicInterestPrimaryCheckView.ServiceId <= 0)
                {
                    HttpContext.Session.Remove("PrimaryCheckData");
                    throw new InvalidOperationException("Primary check session data is missing or contains an invalid service ID.");
                }
                publicInterestPrimaryCheckView.PublicInterestCheckStatus = PublicInterestCheckEnum.PrimaryCheckFailed;
                publicInterestPrimaryCheckView.PrimaryCheckComment = viewModel.PrimaryCheckComment;
                PublicInterestCheckDto publicInterestCheckDto = MapViewModelToDto(publicInterestPrimaryCheckView);
                GenericResponse genericResponse = await publicInterestCheckService.SavePublicInterestCheck(publicInterestCheckDto, ReviewTypeEnum.PrimaryCheck, UserEmail);

                if (genericResponse.Success)
                {
                    return RedirectToAction("PrimaryCheckFailedConfirmation", "PublicInterestPrimaryCheck");
                }
                else
                {
                    HttpContext.Session.Remove("PrimaryCheckData");
                    throw new InvalidOperationException("Failed to save primary check as failed.");
                }
            }
            else
            {
                return View("ConfirmPrimaryCheckFail", viewModel);
            }

        }


        [HttpGet("primary-check-rejection-confirmation")]
        public async Task<IActionResult> PrimaryCheckFailedConfirmation()
        {
            PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckView = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            ServiceDto serviceDto = await publicInterestCheckService.GetServiceDetails(publicInterestPrimaryCheckView.ServiceId);
            HttpContext.Session.Remove("PrimaryCheckData");
            return View(serviceDto);
          
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
                publicInterestPrimaryCheckViewModel.PublicInterestChecksMet= serviceDto.PublicInterestCheck.PublicInterestChecksMet;                
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
                PublicInterestChecksMet= publicInterestPrimaryCheckViewModel.PublicInterestChecksMet,      
                PublicInterestCheckStatus = publicInterestPrimaryCheckViewModel.PublicInterestCheckStatus,               
                PrimaryCheckComment = publicInterestPrimaryCheckViewModel.PublicInterestCheckStatus == PublicInterestCheckEnum.PrimaryCheckFailed?
                publicInterestPrimaryCheckViewModel.PrimaryCheckComment:null,              
                
                PrimaryCheckUserId = Convert.ToInt32(publicInterestPrimaryCheckViewModel.PrimaryCheckUserId)
            };


            return publicInterestCheckDto;
        }

   

        private void AddModelErrorForInvalidActions(PublicInterestPrimaryCheckViewModel publicInterestPrimaryCheckViewModel)
        {          

            if (publicInterestPrimaryCheckViewModel.PrimaryCheckUserId == publicInterestPrimaryCheckViewModel.SecondaryCheckUserId)
            {
                ModelState.AddModelError("SubmitValidation", "Primary and secondary check user should be different");
            }
          
           
        }
       
        #endregion
    }
}
