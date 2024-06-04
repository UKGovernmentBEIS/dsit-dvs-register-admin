using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DVSAdmin.Controllers
{
    [Route("certificate-review")]
    public class CertificateReviewController : Controller
    {

        private readonly ILogger<CertificateReviewController> logger;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IPreRegistrationReviewService preRegistrationReviewService;
        private readonly IUserService userService;
        public CertificateReviewController(ILogger<CertificateReviewController> logger, ICertificateReviewService certificateReviewService, IUserService userService,
            IPreRegistrationReviewService preRegistrationReviewService)
        {
            this.logger = logger;
            this.certificateReviewService = certificateReviewService;
            this.userService = userService;
            this.preRegistrationReviewService = preRegistrationReviewService;
        }

        [HttpGet("certificate-review-list")]
        public async Task<ActionResult> CertificateReviews()
        {

            HttpContext.Session?.Set("Email", "aiswarya.rajendran@dsit.gov.uk");

            
            CertificateReviewListViewModel certificateReviewListViewModel = new CertificateReviewListViewModel();
            var certificateInfoList = await certificateReviewService.GetCertificateInformationList();
            certificateReviewListViewModel.CertificateReviewList = certificateInfoList.Where(x => 
            (x.CertificateInfoStatus == CertificateInfoStatusEnum.Received &&  x.Id !=x?.CertificateReview?.CertificateInformationId)
            ||  (x.CertificateReview !=null && x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.InReview )).ToList();            

            certificateReviewListViewModel.ArchiveList = certificateInfoList.Where( x=> x.CertificateReview !=null && (x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Expired 
            || x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Approved
            || x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Rejected) ).ToList();
            return View(certificateReviewListViewModel);
        }


        [HttpGet("certificate-review-validation")]
        public async Task<ActionResult> CertificateValidation(int certificateInfoId)
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            if (certificateInfoId == 0)
            {
                certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            }
            else
            {
                CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateInfoId);
                certificateValidationViewModel = MapDtoToViewModel(certificateInformationDto);
            }
            return View(certificateValidationViewModel);            
        }


        [HttpPost("certificate-review-validation")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveCertificateValidation(CertificateValidationViewModel certificateValidationViewModel, string saveReview)
        {
            CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateValidationViewModel.CertificateInformationId);
            certificateValidationViewModel.CertificateInformation = certificateInformationDto;           
            certificateValidationViewModel.CommentsForIncorrect = InputSanitizeExtensions.CleanseInput(certificateValidationViewModel.CommentsForIncorrect??string.Empty);
           
            string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
            if (!string.IsNullOrEmpty(loggedinUserEmail))
            {
                if (ModelState.IsValid)
                {
                    HttpContext?.Session.Set("CertificateValidationData", certificateValidationViewModel);
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);
                    CertificateReviewDto certificateReviewDto = MapViewModelToDto(certificateValidationViewModel, userDto.Id, CertificateInfoStatusEnum.InReview, null);
                    GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto,CertificateReviewTypeEnum.Validation);
                   
                    if (saveReview == "draft")
                    {
                        return RedirectToAction("CertificateValidation", new { certificateInfoId = certificateValidationViewModel.CertificateInformationId });
                    }                        
                    else if (saveReview == "continue")
                    {
                        return RedirectToAction("CertificateReview", new { reviewId = genericResponse .InstanceId});
                    }                       
                    else
                    {
                        return RedirectToAction(Constants.ErrorPath);
                    }
                }
                else
                {
                    return View("CertificateValidation", certificateValidationViewModel);
                }
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }
        }

        [HttpGet("certificate-review")]
        public async Task<ActionResult> CertificateReview(int reviewId)
        {

            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();

            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel.CertificateInformation = certificateValidationViewModel.CertificateInformation;
            if (reviewId == 0)
            {
                certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();
            }
            else
            {
                CertificateReviewDto certificateReviewDto = await certificateReviewService.GetCertificateReview(reviewId);
                certificateReviewViewModel.CertificateReviewId = reviewId;
                certificateReviewViewModel.Comments = certificateReviewDto.Comments;
                certificateReviewViewModel.InformationMatched = certificateReviewDto.InformationMatched;
            }
            return View(certificateReviewViewModel);
        }

        [HttpPost("certificate-review")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveCertificateReview(CertificateReviewViewModel certificateReviewViewModel, string saveReview)
        {  
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            certificateReviewViewModel.CertificateInformation = certificateValidationViewModel.CertificateInformation;
            certificateReviewViewModel.Comments =  InputSanitizeExtensions.CleanseInput(certificateReviewViewModel.Comments??string.Empty);

           
            ValidateCertificateReviewViewModel(certificateReviewViewModel, certificateValidationViewModel, saveReview);

            string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
            if (!string.IsNullOrEmpty(loggedinUserEmail))
            {
                if (ModelState.IsValid)
                {
                    HttpContext?.Session.Set("CertificateReviewData", certificateReviewViewModel);
                    CertificateInfoStatusEnum certificateInfoStatus = GetCertificateReviewStatus(saveReview);
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);
                    CertificateReviewDto certificateReviewDto = MapViewModelToDto(certificateValidationViewModel, userDto.Id, certificateInfoStatus, certificateReviewViewModel);
                    if(saveReview == "draft")
                    {
                        GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto, CertificateReviewTypeEnum.InformationMatch);
                        if (genericResponse.Success)
                        {
                            return RedirectToAction("CertificateReview", new { reviewId = certificateReviewViewModel .CertificateReviewId});
                        }
                        else
                        {
                            return RedirectToAction(Constants.ErrorPath);
                        }                      
                    }
                    else if (saveReview == "reject")
                    {
                        HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                        return RedirectToAction("RejectSubmission");
                    }
                    else if (saveReview == "approve")
                    {
                        HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                        return RedirectToAction("ApproveSubmission");
                    }
                    else
                    {
                        return RedirectToAction(Constants.ErrorPath);

                    }
                }
                else
                {
                    return View("CertificateReview", certificateReviewViewModel);
                }
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }

        }


        #region Reject Flow
        [HttpGet("reject-submission")]
        public async Task<ActionResult> RejectSubmission()
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();

            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();

            CertficateRejectionViewModel certficateRejectionViewModel = HttpContext?.Session.Get<CertficateRejectionViewModel>("CertficateRejectionData")??
             new CertficateRejectionViewModel
             {
                 SelectedReasons = new List<CertificateReviewRejectionReasonDto>(),
                 RejectionReasons = await certificateReviewService.GetRejectionReasons()
             };
            certficateRejectionViewModel.CertificateValidation = certificateValidationViewModel;
            certficateRejectionViewModel.CertificateReview = certificateReviewViewModel;
            certficateRejectionViewModel.SelectedRejectionReasonIds = certficateRejectionViewModel?.SelectedReasons?.Select(c => c.Id).ToList();          
            return View(certficateRejectionViewModel);
        }

        [HttpPost("reject-submission")]
        public async Task<ActionResult> SaveRejectSubmission(CertficateRejectionViewModel certficateRejectionViewModel, string saveReview)
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();

            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();

            certficateRejectionViewModel.CertificateValidation = certificateValidationViewModel;
            certficateRejectionViewModel.CertificateReview = certificateReviewViewModel;

            List<CertificateReviewRejectionReasonDto> rejectionReasons = JsonConvert.DeserializeObject<List<CertificateReviewRejectionReasonDto>>(HttpContext.Request.Form["RejectionReasons"]);
            if (rejectionReasons == null  || rejectionReasons.Count == 0)
                rejectionReasons = await certificateReviewService.GetRejectionReasons();
            certficateRejectionViewModel.RejectionReasons = rejectionReasons;
            certficateRejectionViewModel.SelectedRejectionReasonIds =  certficateRejectionViewModel.SelectedRejectionReasonIds??new List<int>();
            if (certficateRejectionViewModel.SelectedRejectionReasonIds.Count > 0)
                certficateRejectionViewModel.SelectedReasons = rejectionReasons.Where(c => certficateRejectionViewModel.SelectedRejectionReasonIds.Contains(c.Id)).ToList();

          
            if (saveReview == "proceed")
            {
                if (ModelState.IsValid)
                {
                    HttpContext?.Session.Set("CertficateRejectionData", certficateRejectionViewModel);
                    CertificateReviewDto certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto");
                    certificateReviewDto.RejectionComments = certficateRejectionViewModel.Comments;
                    ICollection<CertificateReviewRejectionReasonMappingsDto> rejectionReasonIdDtos = new List<CertificateReviewRejectionReasonMappingsDto>();
                    foreach (var item in certficateRejectionViewModel?.SelectedRejectionReasonIds)
                    {
                        rejectionReasonIdDtos.Add(new CertificateReviewRejectionReasonMappingsDto { CertificateReviewRejectionReasonId = item });
                    }
                    certificateReviewDto.CertificateReviewRejectionReasonMappings =  new List<CertificateReviewRejectionReasonMappingsDto>();
                    certificateReviewDto.CertificateReviewRejectionReasonMappings = rejectionReasonIdDtos;
                    HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                    return RedirectToAction("ConfirmRejection"); 
                }
                else
                {
                    return View("RejectSubmission", certficateRejectionViewModel);
                }
            }
            else if(saveReview == "cancel")
            {
                return RedirectToAction("CertificateReview");
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }
        }

        [HttpGet("confirm-rejection")]
        public ActionResult ConfirmRejection()
        {
            return View();
        }

        [HttpPost("confirm-rejection")]
        public async Task<ActionResult> ProceedConfirmRejection(string saveReview)
        {
            if(saveReview == "proceed")
            {
                CertificateReviewDto certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto");
                certificateReviewDto.CertificateInfoStatus = CertificateInfoStatusEnum.Rejected;
                GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto, CertificateReviewTypeEnum.Rejection);
                if (genericResponse.Success)
                {
                    return RedirectToAction("RejectionConfirmation");
                }
                else
                {
                    return RedirectToAction(Constants.ErrorPath);
                }
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("RejectSubmission");
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }
           
        }

        [HttpGet("rejection-confirmation")]
        public ActionResult RejectionConfirmation()
        {
            return View();
        }

        #endregion

        [HttpGet("approve-submission")]
        public async Task<ActionResult> ApproveSubmission()
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();

            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();

            CertificateApprovalViewModel certificateApprovalViewModel = new CertificateApprovalViewModel();
            certificateApprovalViewModel.CertificateReview = certificateReviewViewModel;
            certificateApprovalViewModel.CertificateValidation= certificateValidationViewModel;
            certificateApprovalViewModel.Email =  HttpContext?.Session.Get<string>("Email")??string.Empty;

            PreRegistrationDto preRegistrationDto = await preRegistrationReviewService.GetPreRegistrationDetails(certificateValidationViewModel.PreRegistrationId);
            certificateApprovalViewModel.PreRegistration = preRegistrationDto;
            return View(certificateApprovalViewModel);
        }
        

        [HttpGet("approval-confirmation")]
        public ActionResult ApprovalConfirmation()
        {
            return View();
        }


        [HttpGet("archive-details")]
        public ActionResult ArchiveDetails()
        {
            return View();
        }
      

        //Temp route for building out view
        [HttpGet("certificate-review-submissions")]
        public ActionResult PartialViewsCertificateReviewSubmissionsView()
        {
            return View();
        }
        //Temp route for building out view
        [HttpGet("certificate-review-archive")]
        public ActionResult PartialViewsCertificateReviewArchiveView()
        {
            return View();
        }

        #region Private methods


        private CertificateInfoStatusEnum GetCertificateReviewStatus(string reviewAction)
        {
            if(reviewAction == "approve")
                return CertificateInfoStatusEnum.Approved;
            else if(reviewAction == "reject")
                return CertificateInfoStatusEnum.Rejected;
            else return CertificateInfoStatusEnum.InReview;
        }
        private CertificateReviewDto MapViewModelToDto(CertificateValidationViewModel certificateValidationViewModel, int userId, CertificateInfoStatusEnum reviewStatus, CertificateReviewViewModel? certificateReviewViewModel )
        {
            CertificateReviewDto certificateReviewDto = new CertificateReviewDto();
            certificateReviewDto.PreRegistrationId = certificateValidationViewModel.PreRegistrationId;
            certificateReviewDto.CertificateInformationId =certificateValidationViewModel.CertificateInformationId;

            certificateReviewDto.IsCabLogoCorrect = Convert.ToBoolean(certificateValidationViewModel.IsCabLogoCorrect);
            certificateReviewDto.IsCabDetailsCorrect = Convert.ToBoolean(certificateValidationViewModel.IsCabDetailsCorrect);
            certificateReviewDto.IsProviderDetailsCorrect = Convert.ToBoolean(certificateValidationViewModel.IsProviderDetailsCorrect);
            certificateReviewDto.IsServiceNameCorrect = Convert.ToBoolean(certificateValidationViewModel.IsServiceNameCorrect);
            certificateReviewDto.IsRolesCertifiedCorrect = Convert.ToBoolean(certificateValidationViewModel.IsRolesCertifiedCorrect);
            certificateReviewDto.IsCertificationScopeCorrect = Convert.ToBoolean(certificateValidationViewModel.IsCertificationScopeCorrect);
            certificateReviewDto.IsServiceSummaryCorrect = Convert.ToBoolean(certificateValidationViewModel.IsServiceSummaryCorrect);
            certificateReviewDto.IsURLLinkToServiceCorrect = Convert.ToBoolean(certificateValidationViewModel.IsURLLinkToServiceCorrect);
            certificateReviewDto.IsIdentityProfilesCorrect = Convert.ToBoolean(certificateValidationViewModel.IsIdentityProfilesCorrect);
            certificateReviewDto.IsQualityAssessmentCorrect= Convert.ToBoolean(certificateValidationViewModel.IsQualityAssessmentCorrect);
            certificateReviewDto.IsServiceProvisionCorrect = Convert.ToBoolean(certificateValidationViewModel.IsServiceProvisionCorrect);
            certificateReviewDto.IsLocationCorrect= Convert.ToBoolean(certificateValidationViewModel.IsLocationCorrect);
            certificateReviewDto.IsDateOfIssueCorrect = Convert.ToBoolean(certificateValidationViewModel.IsDateOfIssueCorrect);
            certificateReviewDto.IsDateOfExpiryCorrect = Convert.ToBoolean(certificateValidationViewModel.IsDateOfExpiryCorrect);
            certificateReviewDto.IsAuthenticyVerifiedCorrect = Convert.ToBoolean(certificateValidationViewModel.IsAuthenticyVerifiedCorrect); 
            certificateReviewDto.CommentsForIncorrect =  certificateValidationViewModel.CommentsForIncorrect?? string.Empty;        
            certificateReviewDto.VerifiedUser = userId;
            certificateReviewDto.CertificateInfoStatus = reviewStatus;
            if(certificateReviewViewModel != null)
            {
                certificateReviewDto.Comments = certificateReviewViewModel.Comments;
                certificateReviewDto.InformationMatched = certificateReviewViewModel.InformationMatched;

            }
            
            return certificateReviewDto;

        }

        public static CertificateValidationViewModel MapDtoToViewModel(CertificateInformationDto certificateInformationDto)
        {

            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel.CertificateInformation = certificateInformationDto;
            certificateValidationViewModel.PreRegistrationId = certificateInformationDto.PreRegistrationId;
            certificateValidationViewModel.CertificateInformationId =certificateInformationDto.Id;
            if (certificateInformationDto.CertificateReview!= null)
            {
                certificateValidationViewModel.IsCabLogoCorrect = certificateInformationDto.CertificateReview.IsCabLogoCorrect;
                certificateValidationViewModel.IsCabDetailsCorrect = certificateInformationDto.CertificateReview.IsCabDetailsCorrect;
                certificateValidationViewModel.IsProviderDetailsCorrect = certificateInformationDto.CertificateReview.IsProviderDetailsCorrect;
                certificateValidationViewModel.IsServiceNameCorrect = certificateInformationDto.CertificateReview.IsServiceNameCorrect;
                certificateValidationViewModel.IsRolesCertifiedCorrect = certificateInformationDto.CertificateReview.IsRolesCertifiedCorrect;
                certificateValidationViewModel.IsCertificationScopeCorrect = certificateInformationDto.CertificateReview.IsCertificationScopeCorrect;
                certificateValidationViewModel.IsServiceSummaryCorrect = certificateInformationDto.CertificateReview.IsServiceSummaryCorrect;
                certificateValidationViewModel.IsURLLinkToServiceCorrect = certificateInformationDto.CertificateReview.IsURLLinkToServiceCorrect;
                certificateValidationViewModel.IsIdentityProfilesCorrect = certificateInformationDto.CertificateReview.IsIdentityProfilesCorrect;
                certificateValidationViewModel.IsQualityAssessmentCorrect= certificateInformationDto.CertificateReview.IsQualityAssessmentCorrect;
                certificateValidationViewModel.IsServiceProvisionCorrect =certificateInformationDto.CertificateReview.IsServiceProvisionCorrect;
                certificateValidationViewModel.IsLocationCorrect= certificateInformationDto.CertificateReview.IsLocationCorrect;
                certificateValidationViewModel.IsDateOfIssueCorrect = certificateInformationDto.CertificateReview.IsDateOfIssueCorrect;
                certificateValidationViewModel.IsDateOfExpiryCorrect = certificateInformationDto.CertificateReview.IsDateOfExpiryCorrect;
                certificateValidationViewModel.IsAuthenticyVerifiedCorrect = certificateInformationDto.CertificateReview.IsAuthenticyVerifiedCorrect;
                certificateValidationViewModel.CommentsForIncorrect =  certificateInformationDto.CertificateReview.CommentsForIncorrect;

                
            }
            return certificateValidationViewModel;
        }


        private void ValidateCertificateReviewViewModel(CertificateReviewViewModel certificateReviewViewModel, CertificateValidationViewModel certificateValidationViewModel, string reviewAction)
        {
            bool isInformationMatch = Convert.ToBoolean(certificateReviewViewModel.InformationMatched);
            bool isValidationsCorrect = 
            Convert.ToBoolean(certificateValidationViewModel.IsCabLogoCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsCabDetailsCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsProviderDetailsCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsServiceNameCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsRolesCertifiedCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsCertificationScopeCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsServiceSummaryCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsURLLinkToServiceCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsIdentityProfilesCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsQualityAssessmentCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsServiceProvisionCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsLocationCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsDateOfIssueCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsDateOfExpiryCorrect) &&
            Convert.ToBoolean(certificateValidationViewModel.IsAuthenticyVerifiedCorrect);

            if (isValidationsCorrect && isInformationMatch && reviewAction == "reject")
            {
                ModelState.AddModelError("SubmitValidation", "Your decision to approve or reject must match with the selections");

            }
           else if (!isValidationsCorrect  && !isInformationMatch && reviewAction == "approve")
            {
                ModelState.AddModelError("SubmitValidation", "Certificate validations and information match has rejected options");
                          
            }
            else if( !isValidationsCorrect && reviewAction == "approve")
            {
                ModelState.AddModelError("SubmitValidation", "Certificate validations has rejected options");
            }
            else if (!isInformationMatch && reviewAction == "approve")
            {
                ModelState.AddModelError("SubmitValidation", "Information not matched");
            }
        }
        #endregion
    }
}
