using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSRegister.Extensions;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility;

namespace DVSAdmin.Controllers
{
    [Route("register-management")]
    //Methods/Actions/Views for publishing services
    //Session is used only in PublishService method to keep published service ids
    //There are no user input fields in other methods
    //Any change in the controller routes to be verified 
    //with button or ahref actions in .cshtml and modified 
    public class RegisterManagementController : Controller
    {
        private readonly ILogger<RegisterManagementController> logger;
        private readonly IRegManagementService regManagementService;
        private readonly ICertificateReviewService certificateReviewService;

        public RegisterManagementController(ILogger<RegisterManagementController> logger, IRegManagementService regManagementService,
           ICertificateReviewService certificateReviewService )
        {
            this.logger = logger;
            this.regManagementService = regManagementService;   
            this.certificateReviewService = certificateReviewService;
        }
        [HttpGet("register-management-list")]
        public IActionResult RegisterManagement()
        {
            return View();
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
        public async Task<IActionResult> PublishService(int providerId, bool isReview)
        {
            ViewBag.IsReview = isReview;
            ProviderDto providerDto = await regManagementService.GetProviderWithServiceDeatils(providerId);
            List<CertificateInformationDto> certificateInformation = AssignServiceNumber(providerDto.CertificateInformation);
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;          
            providerDetailsViewModel.Provider.CertificateInformation= certificateInformation
           .Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish).ToList();
            List<int> ServiceIds = providerDetailsViewModel.Provider.CertificateInformation.Select(item => item.Id).ToList();
            HttpContext?.Session.Set("ServiceIdsToPublish", ServiceIds);
            return View(providerDetailsViewModel);
        }


        [HttpGet("proceed-publication")]
        public async Task<IActionResult> ProceedPublication(int providerId)
        {
            ProviderDto providerDto = await regManagementService.GetProviderDetails(providerId);           
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;
            return View(providerDetailsViewModel);
        }

        [HttpGet("about-to-publish")]
        public async Task<IActionResult> AboutToPublish(int providerId)
        {
            string email = HttpContext?.Session.Get<string>("Email")??string.Empty;
            List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish")??new List<int>();
            HttpContext.Session.Remove("ServiceIdsToPublish");
            if(serviceids!=null && serviceids.Any())
            {
                GenericResponse genericResponse = await regManagementService.UpdateServiceStatus(serviceids, providerId, email);
                if (genericResponse.Success)
                {
                    return RedirectToAction("ProviderPublished", new { providerId = providerId });
                }
            }
            return RedirectToAction(Constants.ErrorPath);
        }

        [HttpGet("provider-published")]
        public IActionResult ProviderPublished(int providerid)
        {
            return View();
        }

        #region Private Methods
        private List<CertificateInformationDto> AssignServiceNumber(List<CertificateInformationDto> certificateInformationDtos)
        {
            List<CertificateInformationDto>  certificateInformationDtosFiltered = certificateInformationDtos
           .Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish
           || x.CertificateInfoStatus == CertificateInfoStatusEnum.Published).ToList();
           certificateInformationDtosFiltered.Select((item, index) => new { item, index })
           .ToList().ForEach(x => x.item.ServiceNumber = x.index + 1);
           return certificateInformationDtosFiltered;
        }

        #endregion
    }
}
