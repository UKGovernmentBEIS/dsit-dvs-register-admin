using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
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
                    CertificateInformationDto certificateInformationDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if(certificateInformationDto!= null && certificateInformationDto.CertificateInfoStatus == CommonUtility.Models.Enums.CertificateInfoStatusEnum.ReadyToPublish)
                    {
                        return RedirectToAction(Constants.ErrorPath);
                    }
                    consentViewModel.CertificateInformation = certificateInformationDto;
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
                    CertificateInformationDto certificateInformationDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (ModelState.IsValid)
                    {
                        GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReviewStatus(tokenDetails.Token, tokenDetails.TokenId, certificateInformationDto);
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
                       
                        consentViewModel.CertificateInformation = certificateInformationDto;
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

        [HttpGet("publish-service-give-consent")]
        public async Task<ActionResult> PublishServiceConsent(string token)
        {
            ConsentViewModel consentViewModel = new ConsentViewModel();
            if (!string.IsNullOrEmpty(token))
            {
                consentViewModel.token = token;
                TokenDetails tokenDetails = await jwtService.ValidateToken(token);
                if (tokenDetails != null && tokenDetails.IsAuthorised)
                {
                    CertificateInformationDto certificateInformationDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (certificateInformationDto != null && certificateInformationDto.CertificateInfoStatus == CommonUtility.Models.Enums.CertificateInfoStatusEnum.ReadyToPublish)
                    {
                        return RedirectToAction(Constants.ErrorPath);
                    }
                    consentViewModel.CertificateInformation = certificateInformationDto;
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
        public async Task<ActionResult> PublishServiceGiveConsent(ConsentViewModel consentViewModel)
        {
            if (!string.IsNullOrEmpty(consentViewModel.token))
            {
                TokenDetails tokenDetails = await jwtService.ValidateToken(consentViewModel.token);

                if (tokenDetails != null && tokenDetails.IsAuthorised)
                {
                    CertificateInformationDto certificateInformationDto = await certificateReviewService.GetProviderAndCertificateDetailsByToken(tokenDetails.Token, tokenDetails.TokenId);
                    if (ModelState.IsValid)
                    {
                        GenericResponse genericResponse = await certificateReviewService.UpdateCertificateReviewStatus(tokenDetails.Token, tokenDetails.TokenId, certificateInformationDto);
                        if (genericResponse.Success)
                        {
                            return RedirectToAction("PublishServiceConsentSuccess");
                        }
                        else
                        {
                            return RedirectToAction(Constants.ErrorPath);
                        }
                    }
                    else
                    {

                        consentViewModel.CertificateInformation = certificateInformationDto;
                        return View("PublishServiceConsent", consentViewModel);
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

        [HttpGet("publish-service-consent-success")]
        public ActionResult PublishServiceConsentSuccess()
        {
            return View();
        }


        #region private methods

        private async Task<bool> ValidateToken(string token)
        {
            bool isValid = false;
            TokenDetails tokenDetails = await jwtService.ValidateToken(token);
            if (tokenDetails!= null && tokenDetails.IsAuthorised)
            {
                isValid = true;
            }
            return isValid;

        }
        #endregion
    }
}
