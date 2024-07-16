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
    [ValidCognitoToken]
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
            certificateReviewListViewModel.CertificateReviewList = certificateInfoList.Where(x => 
            (x.CertificateInfoStatus == CertificateInfoStatusEnum.Received &&  x.Id !=x?.CertificateReview?.CertificateInformationId)
            ||  (x.CertificateReview !=null && x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.InReview ) &&x.DaysLeftToComplete>0).ToList();            

            certificateReviewListViewModel.ArchiveList = certificateInfoList.Where( x=>   
            (x.CertificateReview !=null && (x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Approved || x.CertificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Rejected))
             || x.CertificateInfoStatus == CertificateInfoStatusEnum.Expired ).ToList();
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


        [HttpGet("certificate-submission-details")]
        public async Task<ActionResult> CertificateSubmissionDetails(int certificateInfoId)
        {
            CertificateDetailsViewModel certificateDetailsViewModel = new CertificateDetailsViewModel();
            CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateInfoId);
            certificateDetailsViewModel.PreRegistration = certificateInformationDto.Provider.PreRegistration;


            CertificateValidationViewModel certificateValidationViewModel = MapDtoToViewModel(certificateInformationDto);
            CertificateReviewViewModel certificateReviewViewModel = new CertificateReviewViewModel();
            certificateReviewViewModel.CertificateInformation =  MapCertficateInfoDtoToViewModel(certificateInformationDto);
            certificateReviewViewModel.InformationMatched = certificateInformationDto?.CertificateReview?.InformationMatched;
            certificateReviewViewModel.Comments = certificateInformationDto?.CertificateReview?.Comments;

            CertficateRejectionViewModel certficateRejectionViewModel = new CertficateRejectionViewModel();
            certficateRejectionViewModel.CertificateValidation = certificateValidationViewModel;
            certficateRejectionViewModel.CertificateReview = certificateReviewViewModel;
            certficateRejectionViewModel.Comments = certificateInformationDto?.CertificateReview?.RejectionComments;

            certificateDetailsViewModel.CertficateRejection = certficateRejectionViewModel;
            certificateDetailsViewModel.CertificateValidation = certificateValidationViewModel;
            certificateDetailsViewModel.CertificateReview = certificateReviewViewModel;
            return View(certificateDetailsViewModel);          
        }


        [HttpPost("certificate-review-validation")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveCertificateValidation(CertificateValidationViewModel certificateValidationViewModel, string saveReview)
        {
            CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateValidationViewModel.CertificateInformationId);
            certificateValidationViewModel.CertificateInformation = MapCertficateInfoDtoToViewModel(certificateInformationDto);
            certificateValidationViewModel.CommentsForIncorrect = InputSanitizeExtensions.CleanseInput(certificateValidationViewModel.CommentsForIncorrect??string.Empty);
           
            string loggedinUserEmail = HttpContext?.Session.Get<string>("Email");
            if (!string.IsNullOrEmpty(loggedinUserEmail))
            {
                if (ModelState.IsValid)
                {
                    HttpContext?.Session.Set("CertificateValidationData", certificateValidationViewModel);
                    UserDto userDto = await userService.GetUser(loggedinUserEmail);
                    CertificateReviewDto certificateReviewDto = MapViewModelToDto(certificateValidationViewModel, userDto.Id, CertificateInfoStatusEnum.InReview, null);
                    GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto);
                   
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
          
            if (reviewId == 0)
            {
                certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();
            }
            else
            {
                CertificateReviewDto certificateReviewDto = await certificateReviewService.GetCertificateReview(reviewId);
                CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateReviewDto.CertificateInformationId);
                certificateReviewViewModel.CertificateInformation =  MapCertficateInfoDtoToViewModel(certificateInformationDto);
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
                        GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReview(certificateReviewDto,MapCertficateInfoViewModelToDto(certificateReviewViewModel.CertificateInformation));
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
                CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
                certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();

                CertficateRejectionViewModel certficateRejectionViewModel = HttpContext?.Session.Get<CertficateRejectionViewModel>("CertficateRejectionData")??new CertficateRejectionViewModel();

                CertificateReviewDto certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto");
                certificateReviewDto.CertificateInfoStatus = CertificateInfoStatusEnum.Rejected;
                GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReviewRejection(certificateReviewDto, MapCertficateInfoViewModelToDto(certificateValidationViewModel.CertificateInformation), certficateRejectionViewModel.SelectedReasons);
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
        public async Task<ActionResult> RejectionConfirmation()
        {
            CertificateValidationViewModel certificateValidationViewModel = await GetUpdatedCertificateDetails();
            ClearSessionVariables();
            return View(certificateValidationViewModel);
        }

     

        #endregion


        #region Approve Flow

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
            //To get data with updated status
            CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateValidationViewModel.CertificateInformationId);  
            certificateApprovalViewModel.PreRegistration = certificateInformationDto.Provider.PreRegistration;
            return View(certificateApprovalViewModel);
        }
        

        [HttpGet("confirm-approval")]
        public ActionResult ConfirmApproval()
        {
            return View();
        }

        [HttpPost("proceed-approval")]
        public async Task<ActionResult> ProceedApproveSubmission(string saveReview)
        {
            CertificateReviewDto certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto");          

            if(saveReview == "approve")
            {
                HttpContext.Session.Remove("CertificateReviewDto");
                CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
                certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();                             

                certificateReviewDto.CertificateInfoStatus = CertificateInfoStatusEnum.Approved;
                GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReview(certificateReviewDto, MapCertficateInfoViewModelToDto(certificateValidationViewModel.CertificateInformation));
                if (genericResponse.Success)
                {                   
                    return RedirectToAction("ApprovalConfirmation");
                }
                else
                {
                    return RedirectToAction(Constants.ErrorPath);
                }
            }
            else if(saveReview == "cancel")
            {
                return RedirectToAction("ApproveSubmission");
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }
            
        }

        #endregion

        [HttpGet("approval-confirmation")]
        public async Task<ActionResult> ApprovalConfirmation()
        {
            CertificateValidationViewModel certificateValidationViewModel = await GetUpdatedCertificateDetails();
            ClearSessionVariables();
            return View(certificateValidationViewModel);
        }
       

        #region Private methods

        private async Task<CertificateValidationViewModel> GetUpdatedCertificateDetails()
        {
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            CertificateInformationDto certificateInformationDto = await certificateReviewService.GetCertificateInformation(certificateValidationViewModel.CertificateInformationId);
            certificateValidationViewModel.CertificateInformation= MapCertficateInfoDtoToViewModel(certificateInformationDto);
            return certificateValidationViewModel;
        }

        private void ClearSessionVariables()
        {
            HttpContext.Session.Remove("CertificateReviewData");
            HttpContext.Session.Remove("CertificateValidationData");
            HttpContext.Session.Remove("CertificateReviewDto");
            HttpContext.Session.Remove("CertficateRejectionData");
        }

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
            certificateReviewDto.ProviderId = certificateValidationViewModel.CertificateInformation.ProviderId;
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

        private CertificateValidationViewModel MapDtoToViewModel(CertificateInformationDto certificateInformationDto)
        {

            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel.CertificateInformation = MapCertficateInfoDtoToViewModel(certificateInformationDto);
            certificateValidationViewModel.PreRegistrationId = certificateInformationDto.Provider.PreRegistrationId;
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

        private CertificateInformationViewModel MapCertficateInfoDtoToViewModel(CertificateInformationDto certificateInformationDto)
        {
            CertificateInformationViewModel certificateInformationViewModel = new CertificateInformationViewModel();
            certificateInformationViewModel.Id = certificateInformationDto.Id;
            certificateInformationViewModel.ProviderId  = certificateInformationDto.ProviderId;
            certificateInformationViewModel.PreRegistrationId  = certificateInformationDto.Provider.PreRegistration.Id;
           certificateInformationViewModel.PreRegistration  = certificateInformationDto.Provider.PreRegistration;
            certificateInformationViewModel.RegisteredName = certificateInformationDto.Provider.RegisteredName;
            certificateInformationViewModel.TradingName  = certificateInformationDto.Provider.TradingName;
            certificateInformationViewModel.PublicContactEmail = certificateInformationDto.Provider.PublicContactEmail;
            certificateInformationViewModel.TelephoneNumber  = certificateInformationDto.Provider.TelephoneNumber;
            certificateInformationViewModel.WebsiteAddress  = certificateInformationDto.Provider.WebsiteAddress;
            certificateInformationViewModel.Address  = certificateInformationDto.Provider.Address;      
            certificateInformationViewModel.ServiceName  = certificateInformationDto.ServiceName;
            certificateInformationViewModel.FileName  = certificateInformationDto.FileName;
            certificateInformationViewModel.FileLink  = certificateInformationDto.FileLink;
            certificateInformationViewModel.CertificateInfoRoleMapping  = certificateInformationDto.CertificateInfoRoleMapping;
            certificateInformationViewModel.CertificateInfoIdentityProfileMapping  = certificateInformationDto.CertificateInfoIdentityProfileMapping;
            certificateInformationViewModel.HasSupplementarySchemes = certificateInformationDto.HasSupplementarySchemes;
            certificateInformationViewModel.CertificateInfoSupSchemeMappings = certificateInformationDto.CertificateInfoSupSchemeMappings;
            certificateInformationViewModel.ConformityIssueDate = certificateInformationDto.ConformityIssueDate;
            certificateInformationViewModel.ConformityExpiryDate  = certificateInformationDto.ConformityExpiryDate;
            certificateInformationViewModel.CertificateInfoStatus  = certificateInformationDto.CertificateInfoStatus;
            certificateInformationViewModel.CreatedDate  = certificateInformationDto.CreatedDate;           
            certificateInformationViewModel.CreatedBy = certificateInformationDto.CreatedBy;
            certificateInformationViewModel.CertificateReview = certificateInformationDto.CertificateReview;
            certificateInformationViewModel.DaysLeftToComplete = certificateInformationDto.DaysLeftToComplete;
            certificateInformationViewModel.Roles  = certificateInformationDto.Roles;
            certificateInformationViewModel.IdentityProfiles = certificateInformationDto.IdentityProfiles;
            certificateInformationViewModel.SupplementarySchemes  = certificateInformationDto.SupplementarySchemes;
            certificateInformationViewModel.SubmittedCAB = certificateInformationDto.SubmittedCAB;

            return certificateInformationViewModel;


        }

        private CertificateInformationDto MapCertficateInfoViewModelToDto(CertificateInformationViewModel certificateInformationViewModel)
        {
            CertificateInformationDto certificateInformationDto = new CertificateInformationDto();
            certificateInformationDto.Id = certificateInformationViewModel.Id;
            certificateInformationDto.ProviderId  = certificateInformationViewModel.ProviderId;          
            certificateInformationDto.Provider  =  new ProviderDto {
                PreRegistration = certificateInformationViewModel.PreRegistration,
                RegisteredName= certificateInformationViewModel.RegisteredName,
                TradingName = certificateInformationViewModel.TradingName,
                PublicContactEmail = certificateInformationViewModel.PublicContactEmail,
                TelephoneNumber = certificateInformationViewModel.TelephoneNumber,
                WebsiteAddress = certificateInformationViewModel.WebsiteAddress,
                Address = certificateInformationViewModel.Address
            };
            certificateInformationDto.ServiceName  = certificateInformationViewModel.ServiceName;
            certificateInformationDto.CertificateInfoRoleMapping  = certificateInformationViewModel.CertificateInfoRoleMapping;
            certificateInformationDto.CertificateInfoIdentityProfileMapping  = certificateInformationViewModel.CertificateInfoIdentityProfileMapping;
            certificateInformationDto.HasSupplementarySchemes = certificateInformationViewModel.HasSupplementarySchemes;
            certificateInformationDto.CertificateInfoSupSchemeMappings = certificateInformationViewModel.CertificateInfoSupSchemeMappings;
            certificateInformationDto.FileName  = certificateInformationViewModel.FileName;
            certificateInformationDto.FileLink  = certificateInformationViewModel.FileLink;
            certificateInformationDto.ConformityIssueDate = certificateInformationViewModel.ConformityIssueDate;
            certificateInformationDto.ConformityExpiryDate  = certificateInformationViewModel.ConformityExpiryDate;
            certificateInformationDto.CertificateInfoStatus  = certificateInformationViewModel.CertificateInfoStatus;
            certificateInformationDto.CreatedDate  = certificateInformationViewModel.CreatedDate;         
            certificateInformationDto.CreatedBy = certificateInformationViewModel.CreatedBy;
            certificateInformationDto.CertificateReview = certificateInformationViewModel.CertificateReview;
            certificateInformationDto.DaysLeftToComplete = certificateInformationViewModel.DaysLeftToComplete;           
            certificateInformationDto.SubmittedCAB = certificateInformationViewModel.SubmittedCAB;
            return certificateInformationDto;


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

            if (isValidationsCorrect && !isInformationMatch  && reviewAction == "approve")
            {
                ModelState.AddModelError("InformationMatched", "You must select 'Yes, it matches the information on the certificate' in the information match to approve this submission");
            }
            else if (!isValidationsCorrect && reviewAction == "approve")
            {
                ModelState.AddModelError("SubmitValidation", "You must answer 'Correct' for all certificate validation questions to approve this submission");

            }
            else if (isValidationsCorrect && isInformationMatch && reviewAction == "reject")
            {
                ModelState.AddModelError("SubmitValidation", "You cannot reject an application that has passed all certificate validation and information match checks");
            }

        }
        #endregion
    }
}
