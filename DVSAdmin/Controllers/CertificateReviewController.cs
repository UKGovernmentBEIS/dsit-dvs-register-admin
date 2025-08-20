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
        public async Task<ActionResult> CertificateReviews(string currentSort = "days", string currentSortAction = "ascending", int pageNumber = 1, string newSort = "", string searchText = "")
        {
            if (!string.IsNullOrEmpty(newSort))
            {
                if (currentSort.Equals(newSort, StringComparison.OrdinalIgnoreCase))
                    currentSortAction = currentSortAction == "ascending" ? "descending" : "ascending";
                else
                {
                    currentSort = newSort;
                    currentSortAction = "ascending";
                }
                pageNumber = 1;
            }

            var paged = await certificateReviewService.GetCertificateReviews(
                pageNumber, currentSort, currentSortAction, searchText);

            var totalPages = (int)Math.Ceiling(paged.TotalCount / 10d);

            var vm = new CertificateReviewListViewModel
            {
                CertificateReviewList = paged.Items,
                TotalPages = totalPages,
                CurrentSort = currentSort,
                CurrentSortAction = currentSortAction
            };

            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchText = searchText;
            return View(vm);
        }

        [HttpGet("certificate-submission-details")]
        public async Task<ActionResult> CertificateSubmissionDetails(int serviceId)
        {
            SetRefererURL();
            CertificateDetailsViewModel certificateDetailsViewModel = new();
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(serviceId);            
        
            await SetResubmissionFields (serviceDto);
            if (serviceDto == null)
                throw new InvalidOperationException($"Service was not found.");

            if (serviceDto.CertificateReview == null)
                throw new InvalidOperationException($"Service certificate review details are missing.");

            certificateDetailsViewModel.CertificateReview = new ReviewViewModel
            {
                CertificateValid = serviceDto?.CertificateReview.CertificateValid,
                InformationMatched = serviceDto?.CertificateReview.InformationMatched,
                Service = serviceDto
            };

            certificateDetailsViewModel.CertficateRejection = new RejectionViewModel
            {
                Comments = serviceDto?.CertificateReview?.RejectionComments
             };

            certificateDetailsViewModel.SendBackViewModel = new SendBackViewModel
            {
                Reason = serviceDto.CertificateReview.Amendments
            };

          
            certificateDetailsViewModel.CanResendOpeningLoopRequest = serviceDto?.CertificateReview.CertificateReviewStatus == CertificateReviewEnum.Approved
            && (serviceDto.ServiceStatus == ServiceStatusEnum.Submitted
            || serviceDto.ServiceStatus == ServiceStatusEnum.Resubmitted);
            return View(certificateDetailsViewModel);
        }
    

        [HttpGet("certificate-review")]
        public async Task<ActionResult> CertificateReview(int serviceId)
        {
            ReviewViewModel  certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new ();       
       
            ServiceDto service = await certificateReviewService.GetServiceDetails(serviceId);
            if(service != null && (service.ServiceStatus == ServiceStatusEnum.Submitted  || service.ServiceStatus == ServiceStatusEnum.Resubmitted ))
            {
                await SetResubmissionFields(service);
                certificateReviewViewModel.Service = service;
                certificateReviewViewModel.ServiceId = service.Id;
                if (service.CertificateReview != null)
                {
                    certificateReviewViewModel.CertificateValid = service.CertificateReview.CertificateValid;
                    certificateReviewViewModel.InformationMatched = service.CertificateReview.InformationMatched;
                }
                
                return View(certificateReviewViewModel);
            }
            else
            {
                return NotFound();
            }
          
        }

        [HttpPost("certificate-review")]        
        public async Task<ActionResult> SaveCertificateReview(ReviewViewModel certificateReviewViewModel, string saveReview)
        {        
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateReviewViewModel.ServiceId);
            await SetResubmissionFields(serviceDto);
            certificateReviewViewModel.Service = serviceDto;
          
             
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ReviewViewModel", certificateReviewViewModel);
            
                if (saveReview == "continue")
                {
                    if (certificateReviewViewModel.InformationMatched == true && certificateReviewViewModel.CertificateValid == true)
                    {
                        return RedirectToAction("ApproveSubmission");
                    }
                    else
                    {
                        return RedirectToAction("RejectSubmission");
                    }
                }
                else if( saveReview == "send-back")
                {
                    return RedirectToAction("SendBackToCab");
                }
                else
                {
                    throw new InvalidOperationException("Invalid action");
                }
           
                
                
            }
            else
            {
                return View("CertificateReview",certificateReviewViewModel);
            }
          

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
        public  IActionResult ApproveSubmission()
        {
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();               
            return View(certificateReviewViewModel.Service);
        }


        [HttpGet("confirm-approval")]
        public ActionResult ConfirmApproval()
        {
            return View();
        }

        [HttpPost("proceed-approval")]
        public async Task<ActionResult> ProceedApproveSubmission(string saveReview)
        {
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();         
            
            if (saveReview == "approve")
            {
                CertificateReviewDto certificateReviewDto = await MapViewModelToDto(certificateReviewViewModel, CertificateReviewEnum.Approved);
                ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateReviewViewModel.ServiceId);
                GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto, serviceDto, UserEmail);
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
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateReviewViewModel.ServiceId);
            HttpContext?.Session.Remove("ReviewViewModel");
            return View(serviceDto);
        }

        #endregion

        #region Reject Flow

        [HttpGet("reject-submission")]
        public async Task<ActionResult> RejectSubmission()
        {

            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            ServiceDto serviceDto = certificateReviewViewModel.Service;

            RejectionViewModel certficateRejectionViewModel = HttpContext?.Session.Get<RejectionViewModel>("RejectionViewModel")??new();
            certficateRejectionViewModel.InformationMatched = certificateReviewViewModel.InformationMatched;
            certficateRejectionViewModel.CertificateValid = certificateReviewViewModel.CertificateValid;
            certficateRejectionViewModel.Service = certificateReviewViewModel.Service;

            if (certficateRejectionViewModel!=null && certficateRejectionViewModel.SelectedRejectionReasonIds!=null)
            {
                return View(certficateRejectionViewModel);
            }
            else
            {
                certficateRejectionViewModel.SelectedReasons = [];
                certficateRejectionViewModel.RejectionReasons = await certificateReviewService.GetRejectionReasons();
               
                
                if (serviceDto?.CertificateReview != null && serviceDto.CertificateReview.CertificateReviewRejectionReasonMapping != null)
                {
                    CertificateReviewDto certificateReviewDto = await certificateReviewService.GetCertificateReviewWithRejectionData(serviceDto.CertificateReview.Id);
                    //bind previous rejection reason selectopns and comments
                    certficateRejectionViewModel.SelectedRejectionReasonIds = serviceDto.CertificateReview.CertificateReviewRejectionReasonMapping
                     .Select(x => x.CertificateReviewRejectionReasonId).ToList();
                    certficateRejectionViewModel.Comments = serviceDto.CertificateReview.RejectionComments;
                }
                else
                {
                    certficateRejectionViewModel.SelectedRejectionReasonIds = certficateRejectionViewModel?.SelectedReasons?.Select(c => c.Id).ToList();
                }
                return View(certficateRejectionViewModel);
            } 
        }

        [HttpPost("reject-submission")]
        public async Task<ActionResult> SaveRejectSubmission(RejectionViewModel certficateRejectionViewModel, string saveReview)
        {
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            ServiceDto serviceDto = certificateReviewViewModel.Service;

            certficateRejectionViewModel.InformationMatched = certificateReviewViewModel.InformationMatched;
            certficateRejectionViewModel.CertificateValid = certificateReviewViewModel.CertificateValid;
            certficateRejectionViewModel.Service = certificateReviewViewModel.Service;         

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
                    HttpContext?.Session.Set("RejectionViewModel", certficateRejectionViewModel);
                
                    List<CertificateReviewRejectionReasonMappingDto> rejectionReasonIdDtos = [];
                    foreach (var item in certficateRejectionViewModel?.SelectedRejectionReasonIds)
                    {
                        rejectionReasonIdDtos.Add(new CertificateReviewRejectionReasonMappingDto { CertificateReviewRejectionReasonId = item });
                    }
                  
                    return RedirectToAction("ConfirmRejection"); 
                }
                else
                {                    
                    return View("RejectSubmission", certficateRejectionViewModel);
                }
            }
            else if(saveReview == "cancel")
            {
                return RedirectToAction("CertificateReview", new { serviceId = certificateReviewViewModel.ServiceId});
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
                ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
                RejectionViewModel certficateRejectionViewModel = HttpContext?.Session.Get<RejectionViewModel>("RejectionViewModel") ??new RejectionViewModel();
                
                CertificateReviewDto certificateReviewDto = await MapViewModelToDto(certificateReviewViewModel, CertificateReviewEnum.Rejected, certficateRejectionViewModel);
                GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto, certificateReviewViewModel.Service,  UserEmail, certficateRejectionViewModel.SelectedReasons);
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
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateReviewViewModel.ServiceId);
            HttpContext?.Session.Remove("ReviewViewModel");
            HttpContext?.Session.Remove("RejectionViewModel");
            return View(serviceDto);
            
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
                GenericResponse genericResponse = await certificateReviewService.RestoreRejectedCertificateReview(serviceId, UserEmail);
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
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            SendBackViewModel? sendBackViewModel = HttpContext?.Session.Get<SendBackViewModel>("SendBackViewModel") ?? new SendBackViewModel();
            sendBackViewModel.InformationMatched = certificateReviewViewModel.InformationMatched;
            sendBackViewModel.CertificateValid = certificateReviewViewModel.CertificateValid;            
            sendBackViewModel.Service = certificateReviewViewModel.Service;  
            return View(sendBackViewModel);
        }

        [HttpPost("proceed-return")]
        public async Task<ActionResult> ProceedReturn(string action, SendBackViewModel model)
        {
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            model.InformationMatched = certificateReviewViewModel.InformationMatched;
            model.CertificateValid = certificateReviewViewModel.CertificateValid;
            model.Service = certificateReviewViewModel.Service;
           
            if (action == "return")
            {             

                if (ModelState.IsValid)
                {                  

                    CertificateReviewDto certificateReviewDto = await MapViewModelToDto(certificateReviewViewModel, CertificateReviewEnum.AmendmentsRequired, null, model);
                    GenericResponse genericResponse = await certificateReviewService.SaveCertificateReview(certificateReviewDto, certificateReviewViewModel.Service, UserEmail);
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
           
            else
            {
                throw new InvalidOperationException("Invalid send back action.");
            }
        }


        [HttpGet("send-back-confirmation")]
        public async Task<ActionResult> SendBackToCabConfirmation()
        {
            ReviewViewModel certificateReviewViewModel = HttpContext?.Session.Get<ReviewViewModel>("ReviewViewModel") ?? new();
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(certificateReviewViewModel.ServiceId);
            HttpContext?.Session.Remove("ReviewViewModel");
            HttpContext?.Session.Remove("SendBackViewModel");
            return View(serviceDto);
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


        private async Task<CertificateReviewDto> MapViewModelToDto(ReviewViewModel reviewViewModel, CertificateReviewEnum reviewStatus, RejectionViewModel rejectionViewModel = null!, SendBackViewModel sendBackViewModel = null!)
        {
            UserDto userDto = await userService.GetUser(UserEmail);
            CertificateReviewDto certificateReviewDto = new ();
            certificateReviewDto.ServiceId = reviewViewModel.Service.Id;
            certificateReviewDto.ProviProviderProfileId = reviewViewModel.Service.Provider.Id;            
            certificateReviewDto.VerifiedUser = userDto.Id;
            certificateReviewDto.CertificateReviewStatus = reviewStatus;
            certificateReviewDto.InformationMatched = reviewViewModel.InformationMatched;
            certificateReviewDto. CertificateValid= reviewViewModel.CertificateValid;
            if(rejectionViewModel!=null)
            {
                certificateReviewDto.RejectionComments = rejectionViewModel.Comments;
                List<CertificateReviewRejectionReasonMappingDto> rejectionReasonIdDtos = [];
                foreach (var item in rejectionViewModel.SelectedRejectionReasonIds)
                {
                    rejectionReasonIdDtos.Add(new CertificateReviewRejectionReasonMappingDto { CertificateReviewRejectionReasonId = item });
                }
                certificateReviewDto.CertificateReviewRejectionReasonMapping = rejectionReasonIdDtos;
            }
            if(sendBackViewModel!= null)
            {
                certificateReviewDto.Amendments = sendBackViewModel.Reason;
            }
            return certificateReviewDto;

        }
     
        private async Task SetResubmissionFields(ServiceDto serviceDto)
        {
            if (serviceDto.NewOrResubmission == Constants.ReApplication)
            {
                serviceDto.IsResubmission = true;
                ServiceDto previousVersion = await certificateReviewService.GetPreviousServiceVersion(serviceDto.Id);
                serviceDto.PreviousVersionServiceId = previousVersion.Id;
            }
        }
        #endregion
    }
}
