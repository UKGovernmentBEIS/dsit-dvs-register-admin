using Amazon.CognitoIdentityProvider.Model;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using DVSAdmin.Models.CertificateReview;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DVSAdmin.Controllers
{
    
    [Route("certificate-review")]
    public class CertificateReviewController : BaseController
    {      
        private readonly ICertificateReviewService certificateReviewService;      
        private readonly IUserService userService;
        private readonly IBucketService bucketService;
        private readonly IConfiguration configuration;     
        public CertificateReviewController(
            ICertificateReviewService certificateReviewService,
            IUserService userService,
            IBucketService bucketService,
            IConfiguration configuration)
        {           
            this.certificateReviewService = certificateReviewService;
            this.userService = userService; 
            this.bucketService = bucketService;
            this.configuration = configuration;
        }

      

        [HttpGet("certificate-review-list")]
        public async Task<ActionResult> CertificateReviews()
        {            
            CertificateReviewListViewModel certificateReviewListViewModel = new ();
            var serviceList = await certificateReviewService.GetServiceList();

            certificateReviewListViewModel.CertificateReviewList = serviceList
                .Where(x =>
                    ((x.ServiceStatus == ServiceStatusEnum.Submitted || x.ServiceStatus == ServiceStatusEnum.Resubmitted) &&
                     x.ServiceStatus != ServiceStatusEnum.Removed &&
                     x.ServiceStatus != ServiceStatusEnum.SavedAsDraft &&
                     x.Id != x?.CertificateReview?.ServiceId) ||
                        (x.CertificateReview != null &&
                        (x.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.InReview ||
                         x.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.AmendmentsRequired)))
                .OrderBy(x => x.DaysLeftToComplete)
                .ToList();

            certificateReviewListViewModel.ArchiveList = serviceList.Where(x=>x.CertificateReview !=null && 
            ((x.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.Approved) 
            || x.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.Rejected)).OrderByDescending(x => x.CertificateReview.ModifiedDate).ToList();
            return View(certificateReviewListViewModel);
        }

        [HttpGet("certificate-submission-details")]
        public async Task<ActionResult> CertificateSubmissionDetails(int certificateInfoId)
        {
            SetRefererURL();
            CertificateDetailsViewModel certificateDetailsViewModel = new();
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateInfoId);            
        
            if (serviceDto == null)
                throw new InvalidOperationException($"Service was not found.");

            if (serviceDto.CertificateReview == null)
                throw new InvalidOperationException($"Service certificate review details are missing.");

            
            if (serviceDto.ProceedApplicationConsentToken != null & (serviceDto.ServiceStatus == ServiceStatusEnum.Submitted || serviceDto.ServiceStatus == ServiceStatusEnum.Resubmitted) && serviceDto.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.Approved)
            {
                ViewBag.OpeningTheLoopLink = configuration["DvsRegisterLink"] +"consent/proceed-application-consent?token="+serviceDto?.ProceedApplicationConsentToken?.Token;
            }


            CertificateValidationViewModel certificateValidationViewModel = MapDtoToViewModel(serviceDto);
            CertificateReviewViewModel certificateReviewViewModel = new ();
          
            certificateReviewViewModel.InformationMatched = serviceDto?.CertificateReview?.InformationMatched;
            certificateReviewViewModel.Comments = serviceDto?.CertificateReview?.Comments;

            CertficateRejectionViewModel certficateRejectionViewModel = new ();
            certficateRejectionViewModel.CertificateValidation = certificateValidationViewModel;
            certficateRejectionViewModel.CertificateReview = certificateReviewViewModel;
            certficateRejectionViewModel.Comments = serviceDto?.CertificateReview?.RejectionComments;

            SendBackViewModel sendBackViewModel = new();
            sendBackViewModel.Reason = serviceDto.CertificateReview.Amendments;

            certificateDetailsViewModel.CertficateRejection = certficateRejectionViewModel;
            certificateDetailsViewModel.CertificateValidation = certificateValidationViewModel;
            certificateDetailsViewModel.CertificateReview = certificateReviewViewModel;
            certificateDetailsViewModel.SendBackViewModel = sendBackViewModel;
            
            return View(certificateDetailsViewModel);
        }

        [HttpGet("certificate-review-validation")]
        public async Task<ActionResult> CertificateValidation(int serviceId)
        {
            CertificateValidationViewModel certificateValidationViewModel = new();

           
            if (serviceId == 0)
            {
                certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            }
            else
            {
                ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(serviceId);
                
                if (serviceDto == null)
                    throw new InvalidOperationException("Service was not found.");

                if (serviceDto.ServiceStatus == ServiceStatusEnum.Removed || serviceDto.ServiceStatus == ServiceStatusEnum.SavedAsDraft)
                    throw new InvalidOperationException("Service is not in a valid state for certificate review.");
                
                certificateValidationViewModel = MapDtoToViewModel(serviceDto);
            }

            return View(certificateValidationViewModel);
        }    

        [HttpPost("certificate-review-validation")]     
        public async Task<ActionResult> SaveCertificateValidation(CertificateValidationViewModel certificateValidationViewModel, string saveReview)
        {
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateValidationViewModel.ServiceId);
            
            if (serviceDto == null)
                throw new InvalidOperationException("Service was not found.");
            
            certificateValidationViewModel.Service = serviceDto;

            if (string.IsNullOrEmpty(UserEmail) || (saveReview != "draft" && saveReview != "continue"))
                throw new InvalidOperationException("Invalid user or save action provided.");

            HttpContext?.Session.Set("CertificateValidationData", certificateValidationViewModel);
            UserDto userDto = await userService.GetUser(UserEmail);
            CertificateReviewDto certificateReviewDto = MapViewModelToDto(certificateValidationViewModel, userDto.Id, CertificateReviewEnum.InReview, null);

            GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto, UserEmail);
            
            if (!genericResponse.Success)
                throw new InvalidOperationException("Failed to save certificate review details.");

            switch (saveReview)
            {
                case "draft":
                    return RedirectToAction("CertificateValidation", new { serviceId = certificateValidationViewModel?.Service?.Id });
                case "continue":
                    if (!ModelState.IsValid)
                        return View("CertificateValidation", certificateValidationViewModel);
                    return RedirectToAction("CertificateReview", new { reviewId = genericResponse.InstanceId });
                default:
                    throw new InvalidOperationException("Unhandled save action.");
            }
        }

        [HttpGet("certificate-review")]
        public async Task<ActionResult> CertificateReview(int reviewId)
        {           
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            CertificateReviewViewModel certificateReviewViewModel = new();
          
            if (reviewId == 0)
            {
                certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();
            }
            else
            {
                CertificateReviewDto certificateReviewDto = await certificateReviewService.GetCertificateReview(reviewId);
                
                if (certificateReviewDto == null)
                    throw new InvalidOperationException("Certificate review details were not found.");

                if (certificateValidationViewModel.Service == null)
                    throw new InvalidOperationException("Service details are missing from session.");

                certificateReviewViewModel.Service = certificateValidationViewModel.Service;
                certificateReviewViewModel.CertificateReviewId = reviewId;
                certificateReviewViewModel.Comments = certificateReviewDto.Comments;               
                certificateReviewViewModel.InformationMatched = certificateReviewDto.InformationMatched;
                certificateReviewViewModel.Service.CertificateReview = certificateReviewDto;
            }
            return View(certificateReviewViewModel);
        }

        [HttpPost("certificate-review")]        
        public async Task<ActionResult> SaveCertificateReview(CertificateReviewViewModel certificateReviewViewModel, string saveReview)
        {  
           
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            
            if (certificateValidationViewModel.Service == null)
                throw new InvalidOperationException("Service details are missing from session.");

            certificateReviewViewModel.Service = certificateValidationViewModel.Service;
            ValidateCertificateReviewViewModel(certificateReviewViewModel, certificateValidationViewModel, saveReview);

            if (string.IsNullOrEmpty(UserEmail))
                throw new InvalidOperationException("User email is missing from session.");
            
            HttpContext?.Session.Set("CertificateReviewData", certificateReviewViewModel);
            CertificateReviewEnum certificateInfoStatus = GetCertificateReviewStatus(saveReview);
            UserDto userDto = await userService.GetUser(UserEmail);
            
            if (userDto == null)
                throw new InvalidOperationException("User details not found.");
            
            CertificateReviewDto certificateReviewDto = MapViewModelToDto(certificateValidationViewModel, userDto.Id, certificateInfoStatus, certificateReviewViewModel);

            return saveReview switch
            {
                "draft" => await HandleDraftReview(certificateReviewViewModel, certificateReviewDto),
                "reject" => HandleRejectReview(certificateReviewViewModel, certificateReviewDto),
                "approve" => HandleApproveReview(certificateReviewViewModel, certificateReviewDto),
                "send-back" => HandleSendBackToCab(certificateReviewViewModel, certificateReviewDto),
                _ => throw new InvalidOperationException("Invalid review action.")
            };

        }

        [HttpPost("resend-opening-loop-link")]
        public async Task<ActionResult> ResendOpeningLinkEmail(int serviceId)
        {
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(serviceId);
            GenericResponse genericResponse = await certificateReviewService.GenerateTokenAndSendEmail(serviceDto, UserEmail, true);

            if (genericResponse.Success)
            {
                return View("ConsentResentConfirmation");
            }
            return RedirectToAction("CertificateSubmissionDetails", new { serviceId });
        }

        #region Approve Flow

        [HttpGet("approve-submission")]
        public IActionResult ApproveSubmission()
        {           
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();          
            CertificateReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();
            CertificateApprovalViewModel certificateApprovalViewModel = new();
            certificateApprovalViewModel.CertificateReview = certificateReviewViewModel;
            certificateApprovalViewModel.CertificateValidation= certificateValidationViewModel;
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
            
            if (certificateReviewDto == null)
                throw new InvalidOperationException("Certificate review data is missing from session.");
            
            if (saveReview == "approve")
            {
                HttpContext.Session.Remove("CertificateReviewDto");           
                CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
                
                if (certificateValidationViewModel.Service == null)
                    throw new InvalidOperationException("Certificate service details are missing from session.");

                certificateReviewDto.CertificateReviewStatus = CertificateReviewEnum.Approved;
                GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReview(certificateReviewDto, certificateValidationViewModel.Service, UserEmail);
                if (genericResponse.Success)
                {
                    return RedirectToAction("ApprovalConfirmation");
                }
                else
                {
                    throw new InvalidOperationException("Failed to update certificate review during approval.");
                }
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("ApproveSubmission");
            }
            else
            {
                throw new InvalidOperationException("Invalid action for approval submission.");
            }

        }

        [HttpGet("approval-confirmation")]
        public async Task<ActionResult> ApprovalConfirmation()
        {
            CertificateValidationViewModel certificateValidationViewModel = await GetUpdatedCertificateDetails();
            ClearSessionVariables();
            return View(certificateValidationViewModel);
        }

        #endregion

        #region Reject Flow

        [HttpGet("reject-submission")]
        public async Task<ActionResult> RejectSubmission()
        {
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            CertificateReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();
            CertificateReviewDto certificateReviewDto = await certificateReviewService.GetCertificateReviewWithRejectionData(certificateReviewViewModel.CertificateReviewId);

            CertficateRejectionViewModel certficateRejectionViewModel = HttpContext?.Session.Get<CertficateRejectionViewModel>("CertficateRejectionData");
            if (certficateRejectionViewModel!=null && certficateRejectionViewModel.SelectedRejectionReasonIds!=null)
            {
                return View(certficateRejectionViewModel);
            }
            else
            {
                certficateRejectionViewModel =  new CertficateRejectionViewModel
                {
                    SelectedReasons = [],
                    RejectionReasons = await certificateReviewService.GetRejectionReasons(),
                    CertificateValidation = certificateValidationViewModel,
                    CertificateReview = certificateReviewViewModel
                };
                
                if (certificateReviewDto != null && certificateReviewDto.CertificateReviewRejectionReasonMapping != null)
                {
                    //bind previous rejection reason selectopns and comments
                    certficateRejectionViewModel.SelectedRejectionReasonIds = certificateReviewDto.CertificateReviewRejectionReasonMapping
                     .Select(x => x.CertificateReviewRejectionReasonId).ToList();
                    certficateRejectionViewModel.Comments = certificateReviewDto.RejectionComments;
                }
                else
                {
                    certficateRejectionViewModel.SelectedRejectionReasonIds = certficateRejectionViewModel?.SelectedReasons?.Select(c => c.Id).ToList();
                }
                return View(certficateRejectionViewModel);
            } 
        }

        [HttpPost("reject-submission")]
        public async Task<ActionResult> SaveRejectSubmission(CertficateRejectionViewModel certficateRejectionViewModel, string saveReview)
        {
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();           
            CertificateReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData")??new CertificateReviewViewModel();

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
                    
                    if (certificateReviewDto == null)
                        throw new InvalidOperationException("Certificate review data is missing from session.");
                    
                    certificateReviewDto.RejectionComments = certficateRejectionViewModel.Comments;
                    List<CertificateReviewRejectionReasonMappingDto> rejectionReasonIdDtos = [];
                    foreach (var item in certficateRejectionViewModel?.SelectedRejectionReasonIds)
                    {
                        rejectionReasonIdDtos.Add(new CertificateReviewRejectionReasonMappingDto { CertificateReviewRejectionReasonId = item });
                    }
                    certificateReviewDto.CertificateReviewRejectionReasonMapping =  new List<CertificateReviewRejectionReasonMappingDto>();
                    certificateReviewDto.CertificateReviewRejectionReasonMapping = rejectionReasonIdDtos;
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
                throw new InvalidOperationException("Invalid rejection action.");
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
                CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
                CertficateRejectionViewModel certficateRejectionViewModel = HttpContext?.Session.Get<CertficateRejectionViewModel>("CertficateRejectionData")??new CertficateRejectionViewModel();

                CertificateReviewDto certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto");
                
                if (certificateReviewDto == null)
                    throw new InvalidOperationException("Certificate review data is missing from session.");

                certificateReviewDto.CertificateReviewStatus = CertificateReviewEnum.Rejected;
                GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReviewRejection(certificateReviewDto, certificateValidationViewModel.Service, certficateRejectionViewModel.SelectedReasons, UserEmail);
                if (genericResponse.Success)
                {
                    return RedirectToAction("RejectionConfirmation");
                }
                else
                {
                    throw new InvalidOperationException("Failed to update certificate review as rejected.");
                }
            }
            else if (saveReview == "cancel")
            {
                return RedirectToAction("RejectSubmission");
            }
            else
            {
                throw new InvalidOperationException("Invalid rejection confirmation action.");
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

        #region Restore Flow      

        [HttpGet("restore-submission")]
        public IActionResult ConfirmRestoreSubmission(int reviewId, int serviceId)
        {
            ViewBag.ServiceId = serviceId;
            ViewBag.ReviewId = reviewId;
            return View();
        }

        /// <summary>
        /// Save to db
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        [HttpPost("restore-submission")]
        public async Task<IActionResult> RestoreSubmission(int reviewId, int serviceId)
        {

            ServiceDto serviceDetails = await certificateReviewService.GetServiceDetails(serviceId);
            if (serviceDetails.CertificateReview.Id ==reviewId && serviceDetails.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.Rejected)
            {
                GenericResponse genericResponse = await certificateReviewService.RestoreRejectedCertificateReview(reviewId, UserEmail);
                if (genericResponse.Success)
                {
                    return RedirectToAction("RestoreSubmissionConfirmation",new { serviceId = serviceId });
                }
                else
                {
                    return RedirectToAction("RestoreFailed");
                }
            
            }
            else
            {
                return RedirectToAction("RestoreFailed");
            }
           
        }

        [HttpGet("restore-submission-confirmation")]
        public async Task<IActionResult> RestoreSubmissionConfirmation(int serviceId)
        {
            ServiceDto serviceDetails = await certificateReviewService.GetServiceDetails(serviceId);
            return View(serviceDetails);
        }

        [HttpGet("restore-failed")]
        public IActionResult RestoreFailed()
        {
            HttpContext.Session.Clear();
            return View();
        }

        #endregion

        #region Send Back Flow

        [HttpGet("send-back")]
        public IActionResult SendBackToCab()
        {
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData") ?? new CertificateValidationViewModel();
            CertificateReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData") ?? new CertificateReviewViewModel();
            SendBackViewModel? sendBackViewModel = HttpContext?.Session.Get<SendBackViewModel>("SendBackViewModel") ?? new SendBackViewModel();

            if (sendBackViewModel.Reason != null && sendBackViewModel.CertificateValidation != null)
            {
                return View(sendBackViewModel);
            }
            else
            {
                sendBackViewModel = new SendBackViewModel
                {
                    Reason = string.Empty,
                    CommentFromReview = certificateReviewViewModel.Comments,
                    CertificateValidation = certificateValidationViewModel,
                    CertificateReview = certificateReviewViewModel
                };

                HttpContext?.Session.Set("SendBackViewModel", sendBackViewModel);

                return View(sendBackViewModel);
            }
        }

        [HttpPost("proceed-return")]
        public async Task<ActionResult> ProceedReturn(string action, SendBackViewModel model)
        {
            if (action == "return")
            {
                CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData") ?? new CertificateValidationViewModel();
                CertificateReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<CertificateReviewViewModel>("CertificateReviewData") ?? new CertificateReviewViewModel();

                model.CertificateValidation = certificateValidationViewModel;
                model.CertificateReview = certificateReviewViewModel;
                model.CommentFromReview = certificateReviewViewModel.Comments;

                if (ModelState.IsValid)
                {           
                    CertificateReviewDto certificateReviewDto = HttpContext?.Session.Get<CertificateReviewDto>("CertificateReviewDto");
                    
                    if (certificateReviewDto == null)
                        throw new InvalidOperationException("Certificate review data is missing from session.");

                    certificateReviewDto.Amendments = model.Reason;
                    certificateReviewDto.CertificateReviewStatus = CertificateReviewEnum.AmendmentsRequired;


                    GenericResponse genericResponse = await certificateReviewService.UpdateCertificateSentBack(certificateReviewDto, certificateValidationViewModel.Service, UserEmail);
                    if (genericResponse.Success)
                    {
                        return RedirectToAction("SendBackToCabConfirmation");
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to update certificate review as sent back to CAB.");
                    }
                }
                else
                {
                    return View("SendBackToCab", model);
                }
            }
            else if (action == "cancel")
            {
                return RedirectToAction("CertificateReview");
            }
            else
            {
                throw new InvalidOperationException("Invalid send back action.");
            }
        }


        [HttpGet("send-back-confirmation")]
        public async Task<ActionResult> SendBackToCabConfirmation()
        {
            CertificateValidationViewModel certificateValidationViewModel = await GetUpdatedCertificateDetails();
            ClearSessionVariables();
            return View(certificateValidationViewModel);
        }

        #endregion

        /// <summary>
        /// Download from s3
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        [HttpGet("download-certificate")]
        public async Task<IActionResult> DownloadCertificate(string key, string filename)
        {
            try
            {
                byte[]? fileContent = await bucketService.DownloadFileAsync(key);

                if (fileContent == null || fileContent.Length == 0)
                {
                    throw new InvalidOperationException("Downloaded certificate file is empty or not found.");
                }
                string contentType = "application/octet-stream";
                return File(fileContent, contentType, filename);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while downloading the certificate file.", ex);
            }
        }


        #region Private methods


        private ActionResult HandleApproveReview(CertificateReviewViewModel certificateReviewViewModel, CertificateReviewDto certificateReviewDto)
        {
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                return RedirectToAction("ApproveSubmission");

            }
            else
            {
                return View("CertificateReview", certificateReviewViewModel);
            }
        }

        private ActionResult HandleRejectReview(CertificateReviewViewModel certificateReviewViewModel, CertificateReviewDto certificateReviewDto)
        {
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                return RedirectToAction("RejectSubmission");
            }
            else
            {
                return View("CertificateReview", certificateReviewViewModel);
            }
        }

        private async Task<ActionResult> HandleDraftReview(CertificateReviewViewModel certificateReviewViewModel, CertificateReviewDto certificateReviewDto)
        {
            GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReview(certificateReviewDto, certificateReviewViewModel.Service, UserEmail);
            if (genericResponse.Success)
            {
                return RedirectToAction("CertificateReview", new { reviewId = certificateReviewViewModel.CertificateReviewId });
            }
            else
            {
                throw new InvalidOperationException("An error occurred while updating the certificate review.");
            }
        }

        private ActionResult HandleSendBackToCab(CertificateReviewViewModel certificateReviewViewModel, CertificateReviewDto certificateReviewDto)
        {
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("CertificateReviewDto", certificateReviewDto);
                return RedirectToAction("SendBackToCab");
            }
            else
            {
                return View("CertificateReview", certificateReviewViewModel);
            }
        }

        private async Task<CertificateValidationViewModel> GetUpdatedCertificateDetails()
        {
            CertificateValidationViewModel certificateValidationViewModel = HttpContext?.Session.Get<CertificateValidationViewModel>("CertificateValidationData")??new CertificateValidationViewModel();
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateValidationViewModel.ServiceId);
            certificateValidationViewModel.Service = serviceDto;
            return certificateValidationViewModel;
        }

        private void ClearSessionVariables()
        {
            HttpContext.Session.Remove("CertificateReviewData");
            HttpContext.Session.Remove("CertificateValidationData");
            HttpContext.Session.Remove("CertificateReviewDto");
            HttpContext.Session.Remove("CertficateRejectionData");
            HttpContext.Session.Remove("SendBackViewModel");
        }

        private CertificateReviewEnum GetCertificateReviewStatus(string reviewAction)
        {
            if(reviewAction == "approve")
                return CertificateReviewEnum.Approved;
            else if(reviewAction == "reject")
                return CertificateReviewEnum.Rejected;
            else return CertificateReviewEnum.InReview;
        }
        private CertificateReviewDto MapViewModelToDto(CertificateValidationViewModel certificateValidationViewModel, int userId, CertificateReviewEnum reviewStatus, CertificateReviewViewModel? certificateReviewViewModel)
        {
            CertificateReviewDto certificateReviewDto = new CertificateReviewDto();
            certificateReviewDto.ServiceId =certificateValidationViewModel.Service.Id;
            certificateReviewDto.ProviProviderProfileId = certificateValidationViewModel.Service.Provider.Id;
            certificateReviewDto.IsCabLogoCorrect = certificateValidationViewModel.IsCabLogoCorrect;
            certificateReviewDto.IsCabDetailsCorrect = certificateValidationViewModel.IsCabDetailsCorrect;
            certificateReviewDto.IsProviderDetailsCorrect = certificateValidationViewModel.IsProviderDetailsCorrect;
            certificateReviewDto.IsServiceNameCorrect = certificateValidationViewModel.IsServiceNameCorrect;
            certificateReviewDto.IsRolesCertifiedCorrect = certificateValidationViewModel.IsRolesCertifiedCorrect;
            certificateReviewDto.IsCertificationScopeCorrect = certificateValidationViewModel.IsCertificationScopeCorrect;
            certificateReviewDto.IsServiceSummaryCorrect =certificateValidationViewModel.IsServiceSummaryCorrect;
            certificateReviewDto.IsURLLinkToServiceCorrect = certificateValidationViewModel.IsURLLinkToServiceCorrect;
            certificateReviewDto.IsGPG44Correct = certificateValidationViewModel.IsGPG44Correct;
            certificateReviewDto.IsGPG45Correct= certificateValidationViewModel.IsGPG45Correct;
            certificateReviewDto.IsServiceProvisionCorrect = certificateValidationViewModel.IsServiceProvisionCorrect;
            certificateReviewDto.IsLocationCorrect= certificateValidationViewModel.IsLocationCorrect;
            certificateReviewDto.IsDateOfIssueCorrect = certificateValidationViewModel.IsDateOfIssueCorrect;
            certificateReviewDto.IsDateOfExpiryCorrect = certificateValidationViewModel.IsDateOfExpiryCorrect;
            certificateReviewDto.IsAuthenticyVerifiedCorrect = certificateValidationViewModel.IsAuthenticyVerifiedCorrect;
            certificateReviewDto.CommentsForIncorrect =  certificateValidationViewModel.CommentsForIncorrect?? string.Empty;
            certificateReviewDto.VerifiedUser = userId;
            certificateReviewDto.CertificateReviewStatus = reviewStatus;
            if (certificateReviewViewModel != null)
            {
                certificateReviewDto.Comments = certificateReviewViewModel.Comments;
                certificateReviewDto.InformationMatched = certificateReviewViewModel.InformationMatched;

            }

            return certificateReviewDto;

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
            Convert.ToBoolean(certificateValidationViewModel.IsGPG45Correct) &&
             Convert.ToBoolean(certificateValidationViewModel.IsGPG44Correct) &&        
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
            else if (isValidationsCorrect && isInformationMatch && reviewAction == "send-back")
            {
                ModelState.AddModelError("SubmitValidation", "You cannot send an application back to CAB that has passed all certificate validation and information match checks");
            }


        }
        private CertificateValidationViewModel MapDtoToViewModel(ServiceDto serviceDto)
        {

            CertificateValidationViewModel certificateValidationViewModel = new CertificateValidationViewModel();
            certificateValidationViewModel.Service = serviceDto;
            certificateValidationViewModel.ServiceId =serviceDto.Id;
            if (serviceDto.CertificateReview!= null)
            {
                certificateValidationViewModel.IsCabLogoCorrect = serviceDto.CertificateReview.IsCabLogoCorrect;
                certificateValidationViewModel.IsCabDetailsCorrect = serviceDto.CertificateReview.IsCabDetailsCorrect;
                certificateValidationViewModel.IsProviderDetailsCorrect = serviceDto.CertificateReview.IsProviderDetailsCorrect;
                certificateValidationViewModel.IsServiceNameCorrect = serviceDto.CertificateReview.IsServiceNameCorrect;
                certificateValidationViewModel.IsRolesCertifiedCorrect = serviceDto.CertificateReview.IsRolesCertifiedCorrect;
                certificateValidationViewModel.IsCertificationScopeCorrect = serviceDto.CertificateReview.IsCertificationScopeCorrect;
                certificateValidationViewModel.IsServiceSummaryCorrect = serviceDto.CertificateReview.IsServiceSummaryCorrect;
                certificateValidationViewModel.IsURLLinkToServiceCorrect = serviceDto.CertificateReview.IsURLLinkToServiceCorrect;
                certificateValidationViewModel.IsGPG45Correct = serviceDto.CertificateReview.IsGPG45Correct;
                certificateValidationViewModel.IsGPG44Correct = serviceDto.CertificateReview.IsGPG44Correct;
                certificateValidationViewModel.IsServiceProvisionCorrect =serviceDto.CertificateReview.IsServiceProvisionCorrect;
                certificateValidationViewModel.IsLocationCorrect= serviceDto.CertificateReview.IsLocationCorrect;
                certificateValidationViewModel.IsDateOfIssueCorrect = serviceDto.CertificateReview.IsDateOfIssueCorrect;
                certificateValidationViewModel.IsDateOfExpiryCorrect = serviceDto.CertificateReview.IsDateOfExpiryCorrect;
                certificateValidationViewModel.IsAuthenticyVerifiedCorrect = serviceDto.CertificateReview.IsAuthenticyVerifiedCorrect;
                certificateValidationViewModel.CommentsForIncorrect =  serviceDto.CertificateReview.CommentsForIncorrect;


            }
            return certificateValidationViewModel;
        }
        #endregion
    }
}
