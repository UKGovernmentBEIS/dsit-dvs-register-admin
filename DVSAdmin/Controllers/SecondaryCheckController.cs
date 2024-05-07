using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using DVSRegister.Extensions;
using DVSAdmin.CommonUtility;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("secondary-check")]
    public class SecondaryCheckController : Controller
    {
     
        private readonly IPreRegistrationReviewService preRegistrationReviewService;
        private readonly IUserService userService;
        public SecondaryCheckController(IPreRegistrationReviewService preRegistrationReviewService, IUserService userService)
        {           
            this.preRegistrationReviewService = preRegistrationReviewService;
            this.userService = userService;
        }

        /// <summary>
        /// secondary Review screen with approve/reject/sentback
        /// </summary>
        /// <param name="preRegistrationId">The pre registration identifier.</param>
        /// <returns></returns>
        [HttpGet("secondary-check-review")]
        public async Task<IActionResult> SecondaryCheckReview(int preRegistrationId)
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            if (preRegistrationId == 0)
            {
                secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            }
            else
            {
                string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
                if (!string.IsNullOrEmpty(loggedinUserEmail))
                {
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);
                    if(userDto.Id > 0) 
                    {
                      
                        PreRegistrationDto preRegistrationDto = await preRegistrationReviewService.GetPreRegistration(preRegistrationId);
                        secondaryCheckViewModel = MapDtoToViewModel(preRegistrationDto);
                        secondaryCheckViewModel.SecondaryCheckUserId = userDto.Id;
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
            return View(secondaryCheckViewModel);
        }



        /// <summary>
        /// On click / Approve/ Reject/ Sent back button click
        /// re direct to respective pages
        /// </summary>
        /// <param name="secondaryCheckViewModel"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-secondary-check-review")]
        public async Task<IActionResult> SaveSecondaryCheckReview(SecondaryCheckViewModel secondaryCheckViewModel, string saveReview)
        {

            PreRegistrationDto preRegistrationDto = await preRegistrationReviewService.GetPreRegistration(secondaryCheckViewModel.PreRegistrationId);
            SecondaryCheckViewModel secondaryCheckViewModelData = new SecondaryCheckViewModel();
            secondaryCheckViewModelData = MapDtoToViewModel(preRegistrationDto);
            secondaryCheckViewModelData.Comment = InputSanitizeExtensions.CleanseInput(secondaryCheckViewModel.Comment??string.Empty);
            secondaryCheckViewModelData.PreRegistration = preRegistrationDto;
            secondaryCheckViewModelData.SecondaryCheckUserId = secondaryCheckViewModel.SecondaryCheckUserId;

            AddModelErrorForInvalidActions(secondaryCheckViewModelData, saveReview);
            HttpContext?.Session.Set("SecondaryCheckData", secondaryCheckViewModelData);

            if (ModelState.IsValid)
            {
                if (saveReview == "sentback")
                {
                    return RedirectToAction("ConfirmSentBackForPrimaryCheck", "SecondaryCheck");
                }
                else if (saveReview == "reject")
                {
                    return RedirectToAction("ConfirmSecondaryCheckReject", "SecondaryCheck");
                }
                else if (saveReview == "approve")
                {
                    return RedirectToAction("ConfirmIssueURN", "SecondaryCheck");
                }
                else
                {
                    HttpContext?.Session.Remove("SecondaryCheckData");
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {

                return View("SecondaryCheckReview", secondaryCheckViewModelData);
            }
        }



        #region Approve flow

        /// <summary>
        /// Intermediate screen for confirming approval
        /// and release URN
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-urn-issue")]
        public IActionResult ConfirmIssueURN()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            return View(secondaryCheckViewModel);
        }


        /// <summary>
        /// Proceed to approval click from ConfirmIssueURN page
        /// On cancel redirect back to SecondaryCheckReview
        /// </summary>
        /// <param name="preRegistrationReviewViewModelstring"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("proceed-secondary-check-approval")]
        public IActionResult ProceedSecondaryCheckApproval(string saveReview)
        {
            if (saveReview == "approve")
            {
                return RedirectToAction("AboutToIssueURN", "SecondaryCheck");               
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("SecondaryCheckReview", "SecondaryCheck");
            }
            else
            {
                HttpContext.Session.Remove("SecondaryCheckData");
                return RedirectToAction("HandleException", "Error");
            }
        }


        /// <summary>
        /// About to issue urn
        /// </summary>
        /// <returns></returns>

        [HttpGet("about-to-issue-urn")]
        public IActionResult AboutToIssueURN()
        {
            return View();
        }


        /// <summary>
        /// Save Appplication Approval
        /// </summary>
        /// <returns></returns>

        [HttpPost("save-application-approval")]
        public async Task<IActionResult> SaveApplicationApproval()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            GenericResponse genericResponse;
            if (secondaryCheckViewModel != null && secondaryCheckViewModel.PreRegistrationId > 0)
            {
                secondaryCheckViewModel.ApplicationReviewStatus = ApplicationReviewStatusEnum.ApplicationApproved;
                PreRegistrationReviewDto preRegistrationReviewDto = MapViewModelToDto(secondaryCheckViewModel);
                genericResponse = await preRegistrationReviewService.SavePreRegistrationReview(preRegistrationReviewDto, ReviewTypeEnum.SecondaryCheck);                
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }
            if (genericResponse.Success)
            {
                return RedirectToAction("ApplicationApprovalConfirmation", "SecondaryCheck");
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }

        }

        /// <summary>
        /// Final screen for sent back flow
        /// </summary>
        /// <returns></returns>

        [HttpGet("application-approval-confirmation")]
        public IActionResult ApplicationApprovalConfirmation()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            HttpContext.Session.Remove("SecondaryCheckData");
            return View(secondaryCheckViewModel);
        }


        #endregion




        #region Sent back flow

        /// <summary>
        /// Intermediate screen for confirming sent back
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-sent-back")]
        public  IActionResult ConfirmSentBackForPrimaryCheck()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            return View(secondaryCheckViewModel);
        }     

        /// <summary>
        /// Save Application rejected clicked in ConfirmSecondaryCheckReject
        /// On cancel redirect back to ConfirmSecondaryCheckReject
        /// </summary>
        /// <param name="preRegistrationReviewViewModelstring"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-sent-back")]
        public async Task<IActionResult> SaveSentBackForPrimaryCheck(SecondaryCheckViewModel secondaryCheckViewModelFormData, string saveReview)
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
         
            if (secondaryCheckViewModel != null && secondaryCheckViewModel.PreRegistrationId > 0)
            {
                if (saveReview == "sentback")
                {
                    secondaryCheckViewModel.ApplicationReviewStatus = ApplicationReviewStatusEnum.SentBackBySecondReviewer;
                    PreRegistrationReviewDto preRegistrationReviewDto = MapViewModelToDto(secondaryCheckViewModel);
                    GenericResponse genericResponse = await preRegistrationReviewService.SavePreRegistrationReview(preRegistrationReviewDto, ReviewTypeEnum.SecondaryCheck);
                    return RedirectToAction("SentBackConfirmation", "SecondaryCheck");
                }
                else if (saveReview == "cancel")
                {
                    return RedirectToAction("SecondaryCheckReview", "SecondaryCheck");
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
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            HttpContext.Session.Remove("SecondaryCheckData");
            return View(secondaryCheckViewModel);
        }


        #endregion

        /// <summary>
        /// Intermediate screen for confirming secondary check fail
        /// </summary>
        /// <returns></returns>

        [HttpGet("confirm-secondary-check-reject")]
        public IActionResult ConfirmSecondaryCheckReject()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            return View(secondaryCheckViewModel);
        }

       

        /// <summary>
        /// Save Application rejected clicked in ConfirmSecondaryCheckReject
        /// On cancel redirect back to ConfirmSecondaryCheckReject
        /// </summary>
        /// <param name="preRegistrationReviewViewModelstring"></param>
        /// <param name="saveReview"></param>
        /// <returns></returns>
        [HttpPost("save-secondary-check-rejected")]
        public  IActionResult ProceedSecondaryCheckRejection(SecondaryCheckViewModel secondaryCheckViewModelFormData, string saveReview)
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            secondaryCheckViewModel.RejectionReason = secondaryCheckViewModelFormData.RejectionReason;
          
             if (saveReview == "reject")
                {
                    secondaryCheckViewModel.ApplicationReviewStatus = ApplicationReviewStatusEnum.ApplicationRejected;                  
                    AddModelStateErrorIfRejectionReasonNull(secondaryCheckViewModelFormData);

                    if (ModelState.IsValid)
                    {
                    HttpContext?.Session.Set("SecondaryCheckData", secondaryCheckViewModel);
                    return RedirectToAction("AboutToReject", "SecondaryCheck");
                    }
                    else
                    {

                        return View("ConfirmSecondaryCheckReject", secondaryCheckViewModel);
                    }
                }
                else if(saveReview == "cancel")
                {
                    return RedirectToAction("SecondaryCheckReview", "SecondaryCheck");
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

        [HttpGet("about-to-reject")]
        public IActionResult AboutToReject()
        {           
            return View();
        }

        /// <summary>
        /// Save Appplication Rejection
        /// </summary>
        /// <returns></returns>

        [HttpPost("save-rejection")]
        public async Task<IActionResult> SaveApplicationRejection()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            if (secondaryCheckViewModel != null && secondaryCheckViewModel.PreRegistrationId > 0)
            {
                secondaryCheckViewModel.ApplicationReviewStatus = ApplicationReviewStatusEnum.ApplicationRejected;
                PreRegistrationReviewDto preRegistrationReviewDto = MapViewModelToDto(secondaryCheckViewModel);
                GenericResponse genericResponse = await preRegistrationReviewService.SavePreRegistrationReview(preRegistrationReviewDto, ReviewTypeEnum.SecondaryCheck);
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }

            return RedirectToAction("ApplicationRejectionConfirmation", "SecondaryCheck");
        }





        /// <summary>
        /// Final page after rejection
        /// </summary>
        /// <returns></returns>
        [HttpGet("application-rejection-confirmation")]
        public IActionResult ApplicationRejectionConfirmation()
        {
            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel = GetSecondaryCheckDataFromSession(HttpContext, "SecondaryCheckData");
            HttpContext.Session.Remove("SecondaryCheckData");
            return View(secondaryCheckViewModel);
        }

        private void AddModelStateErrorIfRejectionReasonNull(SecondaryCheckViewModel secondaryCheckViewModel)
        {
            if (secondaryCheckViewModel.RejectionReason == null || secondaryCheckViewModel.RejectionReason == 0)
                ModelState.AddModelError("RejectionReason", "Select a reason for rejection");
        }

    

        #region private methods
        private SecondaryCheckViewModel GetSecondaryCheckDataFromSession(HttpContext context, string key)
        {
            SecondaryCheckViewModel model = context?.Session.Get<SecondaryCheckViewModel>(key);
            return model;
        }

        private PreRegistrationReviewDto MapViewModelToDto(SecondaryCheckViewModel secondaryCheckViewModel)
        {
            PreRegistrationReviewDto preRegistrationReviewDto = new PreRegistrationReviewDto();
            preRegistrationReviewDto.PreRegistrationId =secondaryCheckViewModel.PreRegistrationId;
            preRegistrationReviewDto.IsCountryApproved = Convert.ToBoolean(secondaryCheckViewModel?.IsCountryApproved);
            preRegistrationReviewDto.IsCompanyApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsCompanyApproved);
            preRegistrationReviewDto.IsCheckListApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsCheckListApproved);
            preRegistrationReviewDto.IsDirectorshipsApproved = Convert.ToBoolean(secondaryCheckViewModel?.IsDirectorshipsApproved);
            preRegistrationReviewDto.IsDirectorshipsAndRelationApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsDirectorshipsAndRelationApproved);
            preRegistrationReviewDto.IsTradingAddressApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsTradingAddressApproved);
            preRegistrationReviewDto.IsSanctionListApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsSanctionListApproved);
            preRegistrationReviewDto.IsUNFCApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsUNFCApproved);
            preRegistrationReviewDto.IsECCheckApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsECCheckApproved);
            preRegistrationReviewDto.IsTARICApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsTARICApproved);
            preRegistrationReviewDto.IsBannedPoliticalApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsBannedPoliticalApproved);
            preRegistrationReviewDto.IsProvidersWebpageApproved= Convert.ToBoolean(secondaryCheckViewModel?.IsProvidersWebpageApproved);
            preRegistrationReviewDto.Comment = secondaryCheckViewModel?.Comment;
            preRegistrationReviewDto.ApplicationReviewStatus = secondaryCheckViewModel.ApplicationReviewStatus;
            preRegistrationReviewDto.RejectionReason = secondaryCheckViewModel.RejectionReason;
            preRegistrationReviewDto.SecondaryCheckUserId = secondaryCheckViewModel.SecondaryCheckUserId;


            return preRegistrationReviewDto;
        }

        private  SecondaryCheckViewModel MapDtoToViewModel(PreRegistrationDto preRegistrationDto)
        {

            SecondaryCheckViewModel secondaryCheckViewModel = new SecondaryCheckViewModel();
            secondaryCheckViewModel.PreRegistration = preRegistrationDto;

            if (preRegistrationDto.PreRegistrationReview!= null)
            {
                secondaryCheckViewModel.PreRegistration = preRegistrationDto;
                secondaryCheckViewModel.PreRegistrationId = preRegistrationDto.Id;
                secondaryCheckViewModel.IsCountryApproved = preRegistrationDto.PreRegistrationReview.IsCountryApproved;
                secondaryCheckViewModel.IsCompanyApproved=  preRegistrationDto.PreRegistrationReview.IsCompanyApproved;
                secondaryCheckViewModel.IsCheckListApproved= preRegistrationDto.PreRegistrationReview.IsCheckListApproved;
                secondaryCheckViewModel.IsDirectorshipsApproved = preRegistrationDto.PreRegistrationReview.IsDirectorshipsApproved;
                secondaryCheckViewModel.IsDirectorshipsAndRelationApproved= preRegistrationDto.PreRegistrationReview.IsDirectorshipsAndRelationApproved;
                secondaryCheckViewModel.IsTradingAddressApproved= preRegistrationDto.PreRegistrationReview.IsTradingAddressApproved;
                secondaryCheckViewModel.IsSanctionListApproved= preRegistrationDto.PreRegistrationReview.IsSanctionListApproved;
                secondaryCheckViewModel.IsUNFCApproved= preRegistrationDto.PreRegistrationReview.IsUNFCApproved;
                secondaryCheckViewModel.IsECCheckApproved= preRegistrationDto.PreRegistrationReview.IsECCheckApproved;
                secondaryCheckViewModel.IsTARICApproved= preRegistrationDto.PreRegistrationReview.IsTARICApproved;
                secondaryCheckViewModel.IsBannedPoliticalApproved= preRegistrationDto.PreRegistrationReview.IsBannedPoliticalApproved;
                secondaryCheckViewModel.IsProvidersWebpageApproved= preRegistrationDto.PreRegistrationReview.IsProvidersWebpageApproved;
                secondaryCheckViewModel.Comment = preRegistrationDto.PreRegistrationReview.Comment;
                secondaryCheckViewModel.ApplicationReviewStatus = preRegistrationDto.PreRegistrationReview.ApplicationReviewStatus;
                secondaryCheckViewModel.PrimaryCheckUserId = preRegistrationDto.PreRegistrationReview.PrimaryCheckUserId;
            }
            return secondaryCheckViewModel;
        }
        private ApplicationReviewStatusEnum GetApplicationStatus(SecondaryCheckViewModel secondaryCheckViewModel, string saveReview)
        {
            ApplicationReviewStatusEnum applicationReviewStatusEnum = secondaryCheckViewModel.ApplicationReviewStatus;//Default value

            if (saveReview == "approve")
                applicationReviewStatusEnum = ApplicationReviewStatusEnum.ApplicationApproved;
            else if (saveReview == "reject")
                applicationReviewStatusEnum = ApplicationReviewStatusEnum.ApplicationRejected;
            else if (saveReview == "sentback")
                applicationReviewStatusEnum = ApplicationReviewStatusEnum.SentBackBySecondReviewer;
            return applicationReviewStatusEnum;

        }
        private void AddModelErrorForInvalidActions(SecondaryCheckViewModel secondaryCheckViewModel, string reviewAction)
        {
            if (secondaryCheckViewModel.PrimaryCheckUserId == secondaryCheckViewModel.SecondaryCheckUserId)
            {
                ModelState.AddModelError("SubmitValidation", "Primary and secondary check user should be different");
            }
            else if (string.IsNullOrEmpty(secondaryCheckViewModel.Comment))
            {
                ModelState.AddModelError("Comment", "Enter a comment to explain the checks completed");
            }           
        } 
        #endregion
    }
}
