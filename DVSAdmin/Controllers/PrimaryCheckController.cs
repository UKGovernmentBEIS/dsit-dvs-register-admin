using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using DVSRegister.Extensions;


namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("primary-check")]
    public class PrimaryCheckController : Controller
    {
     
        private readonly IPreRegistrationReviewService preRegistrationReviewService;
        public PrimaryCheckController(IPreRegistrationReviewService preRegistrationReviewService)
        {
            this.preRegistrationReviewService = preRegistrationReviewService;
        }

        /// <summary>
        /// Review screen with approve/reject sections
        /// </summary>
        /// <param name="preRegistrationId">The pre registration identifier.</param>
        /// <returns></returns>
        [HttpGet("primary-check-review")]
        public async Task<IActionResult> PrimaryCheckReview(int preRegistrationId)
        {
            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();

            if (preRegistrationId == 0)
            {
                preRegistrationReviewViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            }
            else
            {
                PreRegistrationDto preRegistrationDto = await preRegistrationReviewService.GetPreRegistration(preRegistrationId);
                preRegistrationReviewViewModel = MapDtoToViewModel(preRegistrationDto);
            }

            return View(preRegistrationReviewViewModel);


        }



        /// <summary>
        /// On click of Saveas draft/ Pass/ Fail button click
        /// redirect to respective pages or save as draft
        /// </summary>
        /// <param name="preRegistrationReviewViewModel">The pre registration review view model.</param>
        /// <param name="saveReview">The save review.</param>
        /// <returns></returns>
        [HttpPost("save-primary-check-review")]
        public async Task<IActionResult> SavePrimaryCheckReview(PreRegistrationReviewViewModel preRegistrationReviewViewModel, string saveReview)
        {
            PreRegistrationDto preRegistrationDto = await preRegistrationReviewService.GetPreRegistration(preRegistrationReviewViewModel.PreRegistrationId);
            preRegistrationReviewViewModel.PreRegistration = preRegistrationDto;
            ApplicationReviewStatusEnum reviewStatus = GetApplicationStatus(preRegistrationReviewViewModel, saveReview);
            // To handle back ward navigation between screens and cancel
            HttpContext?.Session.Set("PrimaryCheckData", preRegistrationReviewViewModel); 
            AddModelErrorForInvalidActions(preRegistrationReviewViewModel, saveReview);


            if (ModelState.IsValid)
            {
               
                preRegistrationReviewViewModel.ApplicationReviewStatus = reviewStatus;
                preRegistrationReviewViewModel.Comment = InputSanitizeExtensions.CleanseInput(preRegistrationReviewViewModel.Comment??string.Empty);
                PreRegistrationReviewDto preRegistrationReviewDto = MapViewModelToDto(preRegistrationReviewViewModel);
                if (reviewStatus == ApplicationReviewStatusEnum.InPrimaryReview)
                {
                    GenericResponse genericResponse = await preRegistrationReviewService.SavePreRegistrationReview(preRegistrationReviewDto, ReviewTypeEnum.PrimaryCheck);
                    return RedirectToAction("PrimaryCheckReview", new { preRegistrationId = preRegistrationReviewViewModel.PreRegistration.Id });
                }
                else if (reviewStatus == ApplicationReviewStatusEnum.PrimaryCheckPassed)
                {
                    return RedirectToAction("ConfirmPrimaryCheckPass", "PrimaryCheck");
                }
                else if (reviewStatus == ApplicationReviewStatusEnum.PrimaryCheckFailed)
                {
                    return RedirectToAction("ConfirmPrimaryCheckFail", "PrimaryCheck");
                }
                else
                {
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {

                HttpContext?.Session.Clear();
                return View("PrimaryCheckReview", preRegistrationReviewViewModel);
            }

        }


        /// <summary>
        /// Intermediate screen for confirming primary check fail
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-primary-check-fail")]
        public IActionResult ConfirmPrimaryCheckFail()
        {
            return View();
        }


        /// <summary>
        /// Save When Fail Primary check clicked in ConfirmPrimaryCheckFail page
        /// On cancel redirect back to PrimaryCheckReview
        /// </summary>
        /// <param name="preRegistrationReviewViewModelstring"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-primary-check-failed")]
        public async Task<IActionResult> SavePrimaryCheckFailed(PreRegistrationReviewViewModel preRegistrationReviewViewModelstring, string saveReview)
        {
            //Get data in previous page stored in session , ans save to database
            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();
            preRegistrationReviewViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            if (preRegistrationReviewViewModel != null)
            {
                if (saveReview == "save")
                {
                    if (preRegistrationReviewViewModel != null && preRegistrationReviewViewModel.PreRegistrationId > 0)
                    {
                        PreRegistrationReviewDto preRegistrationReviewDto =MapViewModelToDto(preRegistrationReviewViewModel);
                        preRegistrationReviewDto.ApplicationReviewStatus = ApplicationReviewStatusEnum.PrimaryCheckFailed;
                        GenericResponse genericResponse = await preRegistrationReviewService.SavePreRegistrationReview(preRegistrationReviewDto, ReviewTypeEnum.PrimaryCheck);                      
                        return RedirectToAction("PrimaryCheckFailedConfirmation", "PrimaryCheck");
                    }
                    else
                    {
                        HttpContext.Session.Clear();
                        return RedirectToAction("HandleException", "Error");
                    }
                }
                else if (saveReview == "cancel") // on cancel click go back to previous page with curent data
                {
                    return RedirectToAction("PrimaryCheckReview", "PrimaryCheck");
                }
                else
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return RedirectToAction("HandleException", "Error");
            }

        }


        [HttpGet("primary-check-rejection-confirmation")]
        public IActionResult PrimaryCheckFailedConfirmation()
        {

            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();
            preRegistrationReviewViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            HttpContext.Session.Clear();
            return View(preRegistrationReviewViewModel);
        }



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
        /// <param name="preRegistrationReviewViewModelstring"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-primary-check-passed")]
        public async Task<IActionResult> SavePrimaryCheckPassed(PreRegistrationReviewViewModel preRegistrationReviewViewModelstring, string saveReview)
        {

            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();
            preRegistrationReviewViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            if (preRegistrationReviewViewModel != null)
            {

                if (saveReview == "save")
                {
                    if (preRegistrationReviewViewModel != null && preRegistrationReviewViewModel.PreRegistrationId > 0)
                    {
                        PreRegistrationReviewDto preRegistrationReviewDto =  MapViewModelToDto(preRegistrationReviewViewModel);
                        preRegistrationReviewDto.ApplicationReviewStatus = ApplicationReviewStatusEnum.PrimaryCheckPassed;
                        GenericResponse genericResponse = await preRegistrationReviewService.SavePreRegistrationReview(preRegistrationReviewDto, ReviewTypeEnum.PrimaryCheck);
                        
                        return RedirectToAction("PrimaryCheckPassedConfirmation", "PrimaryCheck");
                    }
                    else
                    {
                        HttpContext.Session.Clear();
                        return RedirectToAction("HandleException", "Error");
                    }
                }
                else if (saveReview == "cancel") // on cancel click go back to previous page with curent data
                {
                    return RedirectToAction("PrimaryCheckReview", "PrimaryCheck");
                }
                else
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return RedirectToAction("HandleException", "Error");
            }

        }


        [HttpGet("primary-check-approval-confirmation")]
        public IActionResult PrimaryCheckPassedConfirmation(PreRegistrationReviewDto preRegistrationReviewDto)
        {
            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();
            preRegistrationReviewViewModel = GetPrimaryCheckDataFromSession(HttpContext, "PrimaryCheckData");
            HttpContext.Session.Clear();
            return View(preRegistrationReviewViewModel);          
        }





        #region Private methods
        public static PreRegistrationReviewDto MapViewModelToDto(PreRegistrationReviewViewModel registrationReviewViewModel)
        {
            PreRegistrationReviewDto preRegistrationReviewDto = new PreRegistrationReviewDto();
            preRegistrationReviewDto.PreRegistrationId =registrationReviewViewModel.PreRegistrationId;
            preRegistrationReviewDto.IsCountryApproved = Convert.ToBoolean(registrationReviewViewModel?.IsCountryApproved);
            preRegistrationReviewDto.IsCompanyApproved= Convert.ToBoolean(registrationReviewViewModel?.IsCompanyApproved);
            preRegistrationReviewDto.IsCheckListApproved= Convert.ToBoolean(registrationReviewViewModel?.IsCheckListApproved);
            preRegistrationReviewDto.IsDirectorshipsApproved = Convert.ToBoolean(registrationReviewViewModel?.IsDirectorshipsApproved);
            preRegistrationReviewDto.IsDirectorshipsAndRelationApproved= Convert.ToBoolean(registrationReviewViewModel?.IsDirectorshipsAndRelationApproved);
            preRegistrationReviewDto.IsTradingAddressApproved= Convert.ToBoolean(registrationReviewViewModel?.IsTradingAddressApproved);
            preRegistrationReviewDto.IsSanctionListApproved= Convert.ToBoolean(registrationReviewViewModel?.IsSanctionListApproved);
            preRegistrationReviewDto.IsUNFCApproved= Convert.ToBoolean(registrationReviewViewModel?.IsUNFCApproved);
            preRegistrationReviewDto.IsECCheckApproved= Convert.ToBoolean(registrationReviewViewModel?.IsECCheckApproved);
            preRegistrationReviewDto.IsTARICApproved= Convert.ToBoolean(registrationReviewViewModel?.IsTARICApproved);
            preRegistrationReviewDto.IsBannedPoliticalApproved= Convert.ToBoolean(registrationReviewViewModel?.IsBannedPoliticalApproved);
            preRegistrationReviewDto.IsProvidersWebpageApproved= Convert.ToBoolean(registrationReviewViewModel?.IsProvidersWebpageApproved);
            preRegistrationReviewDto.Comment = registrationReviewViewModel?.Comment;
            preRegistrationReviewDto.ApplicationReviewStatus = registrationReviewViewModel.ApplicationReviewStatus;
            preRegistrationReviewDto.PrimaryCheckUserId = 1;


            return preRegistrationReviewDto;
        }
        public static PreRegistrationReviewViewModel MapDtoToViewModel(PreRegistrationDto preRegistrationDto)
        {

            PreRegistrationReviewViewModel preRegistrationReviewViewModel = new PreRegistrationReviewViewModel();
            preRegistrationReviewViewModel.PreRegistration = preRegistrationDto;

            if (preRegistrationDto.PreRegistrationReview!= null)
            {
                preRegistrationReviewViewModel.PreRegistration = preRegistrationDto;
                preRegistrationReviewViewModel.IsCountryApproved = preRegistrationDto.PreRegistrationReview.IsCountryApproved;
                preRegistrationReviewViewModel.IsCompanyApproved=  preRegistrationDto.PreRegistrationReview.IsCompanyApproved;
                preRegistrationReviewViewModel.IsCheckListApproved= preRegistrationDto.PreRegistrationReview.IsCheckListApproved;
                preRegistrationReviewViewModel.IsDirectorshipsApproved = preRegistrationDto.PreRegistrationReview.IsDirectorshipsApproved;
                preRegistrationReviewViewModel.IsDirectorshipsAndRelationApproved= preRegistrationDto.PreRegistrationReview.IsDirectorshipsAndRelationApproved;
                preRegistrationReviewViewModel.IsTradingAddressApproved= preRegistrationDto.PreRegistrationReview.IsTradingAddressApproved;
                preRegistrationReviewViewModel.IsSanctionListApproved= preRegistrationDto.PreRegistrationReview.IsSanctionListApproved;
                preRegistrationReviewViewModel.IsUNFCApproved= preRegistrationDto.PreRegistrationReview.IsUNFCApproved;
                preRegistrationReviewViewModel.IsECCheckApproved= preRegistrationDto.PreRegistrationReview.IsECCheckApproved;
                preRegistrationReviewViewModel.IsTARICApproved= preRegistrationDto.PreRegistrationReview.IsTARICApproved;
                preRegistrationReviewViewModel.IsBannedPoliticalApproved= preRegistrationDto.PreRegistrationReview.IsBannedPoliticalApproved;
                preRegistrationReviewViewModel.IsProvidersWebpageApproved= preRegistrationDto.PreRegistrationReview.IsProvidersWebpageApproved;
                preRegistrationReviewViewModel.Comment = preRegistrationDto.PreRegistrationReview.Comment;
                preRegistrationReviewViewModel.ApplicationReviewStatus = preRegistrationDto.PreRegistrationReview.ApplicationReviewStatus;
            }
            return preRegistrationReviewViewModel;
        }
        public static PreRegistrationReviewViewModel GetPrimaryCheckDataFromSession(HttpContext context, string key)
        {
            PreRegistrationReviewViewModel model = context?.Session.Get<PreRegistrationReviewViewModel>(key);
            return model;
        }
     
        private ApplicationReviewStatusEnum GetApplicationStatus(PreRegistrationReviewViewModel pregistrationReviewViewModel, string saveReview)
        {
            ApplicationReviewStatusEnum applicationReviewStatusEnum = pregistrationReviewViewModel.ApplicationReviewStatus;//Default value

            if (saveReview == "approve")
                applicationReviewStatusEnum = ApplicationReviewStatusEnum.PrimaryCheckPassed;
            else if (saveReview == "reject")
                applicationReviewStatusEnum = ApplicationReviewStatusEnum.PrimaryCheckFailed;
            else if (saveReview == "draft")
                applicationReviewStatusEnum = ApplicationReviewStatusEnum.InPrimaryReview;
            return applicationReviewStatusEnum;

        }
        private void AddModelErrorForInvalidActions(PreRegistrationReviewViewModel pregistrationReviewViewModel, string reviewAction)
        {

            if (Convert.ToBoolean(pregistrationReviewViewModel.IsCountryApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsCompanyApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsCheckListApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsDirectorshipsApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsDirectorshipsAndRelationApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsTradingAddressApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsSanctionListApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsUNFCApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsECCheckApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsTARICApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsBannedPoliticalApproved) &&
                Convert.ToBoolean(pregistrationReviewViewModel.IsProvidersWebpageApproved) &&
                reviewAction == "reject")
            {
                ModelState.AddModelError("SubmitValidation", "Your decision to pass or fail this primary check must match with the selections");
            }
            else if ((!Convert.ToBoolean(pregistrationReviewViewModel.IsCountryApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsCompanyApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsCheckListApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsDirectorshipsApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsDirectorshipsAndRelationApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsTradingAddressApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsSanctionListApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsUNFCApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsECCheckApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsTARICApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsBannedPoliticalApproved) ||
             !Convert.ToBoolean(pregistrationReviewViewModel.IsProvidersWebpageApproved))
             &&   reviewAction == "approve")
            {
                ModelState.AddModelError("SubmitValidation", "Your decision to pass or fail this primary check must match with the selections");
            }

            if((reviewAction == "approve" || reviewAction == "reject") &&  string.IsNullOrEmpty( pregistrationReviewViewModel.Comment))            
            {
                ModelState.AddModelError("Comment", "Enter a comment to explain the checks completed");
            }
        }
        #endregion
    }
}
