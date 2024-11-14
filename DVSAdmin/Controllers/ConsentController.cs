using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("consent")]
    public class ConsentController : Controller
    {
        private readonly IJwtService jwtService;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IPublicInterestCheckService publicInterestCheckService;
        private readonly IConsentService consentService;
        private string UserEmail => HttpContext.Session.Get<string>("Email")??string.Empty;

        public ConsentController(IJwtService jwtService, ICertificateReviewService certificateReviewService, IConsentService consentService, IPublicInterestCheckService publicInterestCheckService)
        {
            this.jwtService = jwtService;
            this.certificateReviewService = certificateReviewService;
            this.consentService = consentService;
            this.publicInterestCheckService=publicInterestCheckService;
        }


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
                    consentViewModel.Service = ServiceDto;
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
            string email = UserEmail;
            if (!string.IsNullOrEmpty(consentViewModel.token))
            {
                TokenDetails tokenDetails = await jwtService.ValidateToken(consentViewModel.token);

                if (tokenDetails != null && tokenDetails.IsAuthorised)
                {
                    ServiceDto serviceDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);                  
                    email = string.IsNullOrEmpty(email) ? serviceDto.Provider.PrimaryContactEmail + ";"+ serviceDto.Provider.SecondaryContactEmail : email;

                    if (ModelState.IsValid)
                    {
                        GenericResponse genericResponse = await certificateReviewService.UpdateServiceStatus(serviceDto.Id, email);
                        if (genericResponse.Success)
                        {
                            await consentService.RemoveProceedApplicationConsentToken(tokenDetails.Token, tokenDetails.TokenId, email);
                            return RedirectToAction("ProceedApplicationConsentSuccess");
                        }
                        else
                        {
                            return RedirectToAction("ProceedApplicationConsentError");
                        }
                    }
                    else
                    {

                        consentViewModel.Service = serviceDto;
                        return View("ProceedApplicationConsent", consentViewModel);
                    }
                }
                else
                {
                    await consentService.RemoveProceedApplicationConsentToken(tokenDetails.Token, tokenDetails.TokenId, email??"");
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


        #region Closing the loop        

        [HttpGet("publish-service-give-consent")]
        public  async Task<ActionResult> Consent(string token)
        {
            ConsentViewModel consentViewModel = new ConsentViewModel();
            if (!string.IsNullOrEmpty(token))
            {
                consentViewModel.token =  token;
                TokenDetails tokenDetails = await jwtService.ValidateToken(token);
                if (tokenDetails!= null && tokenDetails.IsAuthorised)
                {
                    ServiceDto ServiceDto = await publicInterestCheckService.GetProviderAndCertificateDetailsByConsentToken(tokenDetails.Token, tokenDetails.TokenId);
                    if(ServiceDto!= null && ServiceDto.ServiceStatus == ServiceStatusEnum.ReadyToPublish)
                    {
                        return RedirectToAction(Constants.ErrorPath);
                    }
                    consentViewModel.Service = ServiceDto;
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

        [HttpPost("publish-service-give-consent")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiveConsent(ConsentViewModel consentViewModel)
        {
            if(!string.IsNullOrEmpty(consentViewModel.token))
            {
                TokenDetails tokenDetails = await jwtService.ValidateToken(consentViewModel.token);
               
                if(tokenDetails!= null && tokenDetails.IsAuthorised)
                {
                    ServiceDto ServiceDto = await publicInterestCheckService.GetProviderAndCertificateDetailsByConsentToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (ModelState.IsValid)
                    {
                        GenericResponse genericResponse = await publicInterestCheckService.UpdateServiceAndProviderStatus(tokenDetails.Token, tokenDetails.TokenId, ServiceDto);
                        if (genericResponse.Success)
                        {
                            await consentService.RemoveConsentToken(tokenDetails.Token, tokenDetails.TokenId);
                            return RedirectToAction("ConsentSuccess");
                        }
                        else
                        {
                            return RedirectToAction(Constants.ErrorPath);
                        } 
                    }
                    else
                    {
                       
                        consentViewModel.Service = ServiceDto;
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

    


    }
}
