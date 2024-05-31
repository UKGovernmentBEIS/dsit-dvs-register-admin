using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Extensions;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DVSAdmin.Controllers
{
    [Route("certificate-review")]
    public class CertificateReviewController : Controller
    {

        private readonly ILogger<CertificateReviewController> logger;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IUserService userService;
        public CertificateReviewController(ILogger<CertificateReviewController> logger, ICertificateReviewService certificateReviewService, IUserService userService)
        {
            this.logger = logger;
            this.certificateReviewService = certificateReviewService;
            this.userService = userService;
        }

        [HttpGet("certificate-review-list")]
        public async Task<ActionResult> CertificateReviews()
        {
            
            CertificateReviewListViewModel certificateReviewListViewModel = new CertificateReviewListViewModel();
            var certificateInfoList = await certificateReviewService.GetCertificateInformationList();
            certificateReviewListViewModel.CertificateReviewList = certificateInfoList.Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.Received
            ||  (x.CertificateReview !=null && x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.InReview)).ToList();            

            certificateReviewListViewModel.ArchiveList = certificateInfoList.Where( x=> x.CertificateReview !=null && (x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Expired 
            || x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Approved) ).ToList();
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
            HttpContext?.Session.Set("CertificateValidationData", certificateValidationViewModel);
            certificateValidationViewModel.CommentsForIncorrect = InputSanitizeExtensions.CleanseInput(certificateValidationViewModel.CommentsForIncorrect??string.Empty);
            string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
            if (!string.IsNullOrEmpty(loggedinUserEmail))
            {
                if (ModelState.IsValid)
                {
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);
                    CertificateReviewDto certificateReviewDto = MapViewModelToDto(certificateValidationViewModel, userDto.Id);
                    GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto);
                   
                    if (saveReview == "draft")
                    {
                        return RedirectToAction("CertificateValidation", new { certificateInfoId = certificateValidationViewModel.CertificateInformationId });
                    }
                        
                    else if (saveReview == "continue")
                    {
                        HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                        return RedirectToAction("CertificateReview", new { reviewId = genericResponse.InstanceId });
                    }                       
                    else
                    {
                        return RedirectToAction("HandleException", "Error");
                    }
                   
                }
                else
                {
                    return View("CertificateValidation", certificateValidationViewModel);
                }
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }
           
        }

        [HttpGet("certificate-review")]
        public ActionResult CertificateReview()
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();



            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel.CertificateInformation = certificateValidationViewModel.CertificateInformation;
          

            return View(certificateReviewViewModel);
        }

        [HttpPost("certificate-review")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveCertificateReview(CertificateReviewViewModel certificateReviewViewModel, string saveReview)
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            certificateReviewViewModel.CertificateInformation =certificateValidationViewModel.CertificateInformation;
            
            ValidateCertificateReviewViewModel(certificateReviewViewModel, certificateValidationViewModel, saveReview);          


            if (ModelState.IsValid)
            {
                CertificateReviewDto certificateReviewDto = new CertificateReviewDto();
                certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto")??new CertificateReviewDto();
                certificateReviewDto.InformationMatched = certificateReviewViewModel.InformationMatched;
                certificateReviewDto.Comments = certificateReviewViewModel.Comments;
                
                if (saveReview == "draft")
                {
                    certificateReviewDto.CertificateInfoStatus = CertificateInfoStatusEnum.InReview;
                    GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto);
                    return View(certificateReviewViewModel);
                }
                else if (saveReview == "approve")
                {
                    
                    return RedirectToAction("ApproveSubmission");
                }
                else if (saveReview == "reject")
                {
                    HttpContext?.Session.Set("CertificateReviewViewData", certificateReviewViewModel);
                    return RedirectToAction("RejectSubmission");
                }
                else
                {
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                return View("CertificateReview", certificateReviewViewModel);
            }
        }

        [HttpGet("reject-submission")]
        public async Task<ActionResult> RejectSubmission()
        {
            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();

            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewViewData")??new CertificateReviewViewModel();


            CertficateRejectionViewModel certficateRejectionViewModel = new CertficateRejectionViewModel();
            certficateRejectionViewModel.CertificateValidation = certificateValidationViewModel;
            certficateRejectionViewModel.CertificateReview = certificateReviewViewModel;
            certficateRejectionViewModel.SelectedReasons = new List<CertificateReviewRejectionReasonDto>();
            certficateRejectionViewModel.RejectionReasons = await certificateReviewService.GetRejectionReasons();
            certficateRejectionViewModel.SelectedRejectionReasonIds = certficateRejectionViewModel?.SelectedReasons?.Select(c => c.Id).ToList();
            return View(certficateRejectionViewModel);
        }


        

        [HttpGet("approve-submission")]
        public ActionResult ApproveSubmission()
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
        private CertificateReviewDto MapViewModelToDto(CertificateValidationViewModel certificateValidationViewModel, int userId )
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
            certificateReviewDto.CertificateInfoStatus = CertificateInfoStatusEnum.InReview;
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
            

            if(Convert.ToBoolean(certificateReviewViewModel.InformationMatched) &&
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
            Convert.ToBoolean(certificateValidationViewModel.IsAuthenticyVerifiedCorrect) && reviewAction == "reject")
            {
                ModelState.AddModelError("SubmitValidation", "Your decision to approve or reject must match with the selections");

            }
           else if ((!Convert.ToBoolean(certificateReviewViewModel.InformationMatched) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsCabLogoCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsCabDetailsCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsProviderDetailsCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsServiceNameCorrect) &&
           !Convert.ToBoolean(certificateValidationViewModel.IsRolesCertifiedCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsCertificationScopeCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsServiceSummaryCorrect) &&
           !Convert.ToBoolean(certificateValidationViewModel.IsURLLinkToServiceCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsIdentityProfilesCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsQualityAssessmentCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsServiceProvisionCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsLocationCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsDateOfIssueCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsDateOfExpiryCorrect) ||
           !Convert.ToBoolean(certificateValidationViewModel.IsAuthenticyVerifiedCorrect) )&& reviewAction == "approve")
            {
                ModelState.AddModelError("SubmitValidation", "Your decision to approve or reject must match with the selections");

            }



        }



        #endregion
    }
}
