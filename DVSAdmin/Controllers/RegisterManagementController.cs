using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models.RegManagement;
using DVSRegister.Extensions;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("register-management")]
    //Methods/Actions/Views for publishing services
    //Session is used only in PublishService method to keep published service ids
    //as there are no user input fields in other methods
    //Any change in the controller routes to be verified
    //with button or ahref actions in .cshtml
    public class RegisterManagementController : Controller
    {
        private readonly ILogger<RegisterManagementController> logger;
        private readonly IRegManagementService regManagementService;
        private readonly ICertificateReviewService certificateReviewService;

        public RegisterManagementController(ILogger<RegisterManagementController> logger, IRegManagementService regManagementService,
           ICertificateReviewService certificateReviewService)
        {
            this.logger = logger;
            this.regManagementService = regManagementService;
            this.certificateReviewService = certificateReviewService;
        }

        [HttpGet("register-management-list")]
        public async Task<IActionResult> RegisterManagement()
        {
            ProviderListViewModel providerListViewModel = new ProviderListViewModel();
            var providersList = await regManagementService.GetProviders();
            providerListViewModel.ActionRequiredList = providersList.Where(x => x.ProviderStatus == ProviderStatusEnum.ActionRequired 
            ||  x.ProviderStatus == ProviderStatusEnum.PublishedActionRequired).ToList();
            providerListViewModel.PublicationCompleteList = providersList.Where(x => x.ProviderStatus == ProviderStatusEnum.Published).ToList();
            return View(providerListViewModel);
        }
        [HttpGet("provider-details")]
        public async Task<IActionResult> ProviderDetails(int providerId)
        {
            ProviderDto providerDto = await regManagementService.GetProviderDetails(providerId);
            providerDto.CertificateInformation = AssignServiceNumber(providerDto.CertificateInformation);
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;
            return View(providerDetailsViewModel);
        }

        [HttpGet("service-details")]
        public async Task<IActionResult> ServiceDetails(int serviceId, int serviceNumber)
        {
            CertificateInformationDto certificateInformation = await certificateReviewService.GetCertificateInformation(serviceId);
            ServiceDetailsViewModel serviceDetailsViewModel = new ServiceDetailsViewModel();
            serviceDetailsViewModel.CertificateInformation = certificateInformation;
            serviceDetailsViewModel.ServiceNumber = serviceNumber;
            return View(serviceDetailsViewModel);
        }

        /// <summary>
        /// This method is used to show 2 pages
        /// for viewing services and review before publish
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="isReview"></param>
        /// <returns></returns>
        [HttpGet("publish-service")]
        public async Task<IActionResult> PublishService(int providerId)
        {
           
            ProviderDto providerDto = await regManagementService.GetProviderWithServiceDeatils(providerId);
            List<CertificateInformationDto> certificateInformation = AssignServiceNumber(providerDto.CertificateInformation);
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;
            providerDetailsViewModel.Provider.CertificateInformation = certificateInformation
           .Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish).ToList();
            List<int> ServiceIds = providerDetailsViewModel.Provider.CertificateInformation.Select(item => item.Id).ToList();
            HttpContext?.Session.Set("ServiceIdsToPublish", ServiceIds);
              
            return View(providerDetailsViewModel);
        }


        [HttpGet("proceed-publication")]
        public async Task<IActionResult> ProceedPublication(int providerId)
        {
            //To make sure only the service ids reviewed in previous screen is fetched
            List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish") ?? new List<int>();          
            ProviderDto providerDto = await regManagementService.GetProviderDetails(providerId);
            providerDto.CertificateInformation =  providerDto.CertificateInformation.Where(item => serviceids.Contains(item.Id)).ToList();
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;
            return View(providerDetailsViewModel);
        }

        [HttpGet("about-to-publish")]
        public ActionResult AboutToPublish(int providerId)
        {         
           ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
           providerDetailsViewModel.Provider= new ProviderDto { Id = providerId }; 
           return View(providerDetailsViewModel);
        }

        [HttpPost("about-to-publish")]
        public async Task<IActionResult> Publish(ProviderDetailsViewModel providerDetailsViewModel, string action)
        {
            if(action == "publish")
            {
                string email = HttpContext?.Session.Get<string>("Email") ?? string.Empty;
                List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish") ?? new List<int>();
                if (serviceids != null && serviceids.Any())
                {
                    GenericResponse genericResponse = await regManagementService.UpdateServiceStatus(serviceids, providerDetailsViewModel.Provider.Id, email);
                    if (genericResponse.Success)
                    {
                        return RedirectToAction("ProviderPublished", new { providerId  = providerDetailsViewModel.Provider.Id });

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
            else if(action == "cancel")
            {
                return RedirectToAction("ProceedPublication", new { providerId = providerDetailsViewModel.Provider.Id });
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }           
         
        }

        [HttpGet("provider-published")]
        public async Task<IActionResult> ProviderPublished(int providerId)
        {
            ProviderDto providerDto = await regManagementService.GetProviderWithServiceDeatils(providerId);          
            List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish") ?? new List<int>();
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;
            providerDetailsViewModel.Provider.CertificateInformation = providerDto.CertificateInformation
           .Where(x => serviceids.Contains(x.Id)).ToList();
            HttpContext.Session.Remove("ServiceIdsToPublish");
            return View(providerDetailsViewModel);
        }

        #region Private Methods
        private List<CertificateInformationDto> AssignServiceNumber(List<CertificateInformationDto> certificateInformationDtos)
        {
            List<CertificateInformationDto> certificateInformationDtosFiltered = certificateInformationDtos
           .Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish
           || x.CertificateInfoStatus == CertificateInfoStatusEnum.Published).ToList();
            certificateInformationDtosFiltered.Select((item, index) => new { item, index })
            .ToList().ForEach(x => x.item.ServiceNumber = x.index + 1);
            return certificateInformationDtosFiltered;
        }

        #endregion
    }
}
