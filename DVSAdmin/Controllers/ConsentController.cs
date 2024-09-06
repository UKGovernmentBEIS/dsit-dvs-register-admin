using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("consent")]
    public class ConsentController : Controller
    {
        private readonly IJwtService jwtService;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IConsentService consentService;

        public ConsentController(IJwtService jwtService, ICertificateReviewService certificateReviewService, IConsentService consentService)
        {
            this.jwtService = jwtService;
            this.certificateReviewService = certificateReviewService;
            this.consentService = consentService;
        }


        #region Closing the loop
        //To do : rename as - publish service consent during register management

        [HttpGet("give-consent")]
        public  async Task<ActionResult> Consent(string token)
        {
            ConsentViewModel consentViewModel = new ConsentViewModel();
            if (!string.IsNullOrEmpty(token))
            {
                consentViewModel.token =  token;
                TokenDetails tokenDetails = await jwtService.ValidateToken(token);
                if (tokenDetails!= null && tokenDetails.IsAuthorised)
                {
                    ServiceDto ServiceDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if(ServiceDto!= null && ServiceDto.ServiceStatus == ServiceStatusEnum.Received)
                    {
                        return RedirectToAction(Constants.ErrorPath);
                    }
                    consentViewModel.CertificateInformation = ServiceDto;
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
               
           
            return View(consentViewModel);
        }

        [HttpPost("give-consent")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiveConsent(ConsentViewModel consentViewModel)
        {
            if(!string.IsNullOrEmpty(consentViewModel.token))
            {
                TokenDetails tokenDetails = await jwtService.ValidateToken(consentViewModel.token);
               
                if(tokenDetails!= null && tokenDetails.IsAuthorised)
                {
                    ServiceDto ServiceDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (ModelState.IsValid)
                    {
                        GenericResponse genericResponse = new(); //TO DO : update during register management changes
                        if (genericResponse.Success)
                        {
                            return RedirectToAction("ConsentSuccess");
                        }
                        else
                        {
                            return RedirectToAction(Constants.ErrorPath);
                        } 
                    }
                    else
                    {
                       
                        consentViewModel.CertificateInformation = ServiceDto;
                        return View("Consent", consentViewModel);
                    }
                }
                else
                {
                    await consentService.RemoveConsentToken(tokenDetails.Token, tokenDetails.TokenId);
                    return RedirectToAction(Constants.ErrorPath);
                }
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }         
            
        }

        [HttpGet("consent-success")]
        public ActionResult ConsentSuccess()
        {
            return View();
        }
        #endregion

        #region Opening Loop

        [HttpGet("proceed-application-consent")]
        public async Task<ActionResult> ProceedApplicationConsent(string token)
        {
            ConsentViewModel consentViewModel = new();
            if (!string.IsNullOrEmpty(token))
            {
                consentViewModel.token = token;
                TokenDetails tokenDetails = await jwtService.ValidateToken(token);
                if (tokenDetails != null && tokenDetails.IsAuthorised)
                {
                    ServiceDto ServiceDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (ServiceDto != null && (ServiceDto.ServiceStatus == ServiceStatusEnum.Received ||ServiceDto.CertificateReview.CertificateReviewStatus != CertificateReviewEnum.Approved))
                    {
                        return RedirectToAction("ProceedApplicationConsentError");
                    }
                    consentViewModel.CertificateInformation = ServiceDto;
                }
                else
                {
                    return RedirectToAction("ProceedApplicationConsentError");
                }
            }
            else
            {
                return RedirectToAction("ProceedApplicationConsentError");
            }


            return View(consentViewModel);
        }

        [HttpPost("proceed-application-consent")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProceedApplicationGiveConsent(ConsentViewModel consentViewModel)
        {
            if (!string.IsNullOrEmpty(consentViewModel.token))
            {
                TokenDetails tokenDetails = await jwtService.ValidateToken(consentViewModel.token);

                if (tokenDetails != null && tokenDetails.IsAuthorised)
                {
                    ServiceDto serviceDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (ModelState.IsValid)
                    {
                        GenericResponse genericResponse = await certificateReviewService.UpdateServiceStatus(serviceDto.Id);
                        if (genericResponse.Success)
                        {
                            return RedirectToAction("ProceedApplicationConsentSuccess");
                        }
                        else
                        {
                            return RedirectToAction("ProceedApplicationConsentError");
                        }
                    }
                    else
                    {

                        consentViewModel.CertificateInformation = serviceDto;
                        return View("ProceedApplicationConsent", consentViewModel);
                    }
                }
                else
                {
                    await consentService.RemoveConsentToken(tokenDetails.Token, tokenDetails.TokenId);
                    return RedirectToAction("ProceedApplicationConsentError");
                }
            }
            else
            {
                return RedirectToAction("ProceedApplicationConsentError");
            }

        }

        [HttpGet("proceed-application-consent-success")]
        public ActionResult ProceedApplicationConsentSuccess()
        {
            return View();
        }

        [HttpGet("proceed-application-consent-error")]
        public ActionResult ProceedApplicationConsentError()
        {
            return View();
        }
        #endregion


    }
}
