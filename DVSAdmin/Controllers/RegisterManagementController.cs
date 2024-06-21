using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.Controllers
{
    [Route("register-management")]
    public class RegisterManagementController : Controller
    {
        private readonly ILogger<RegisterManagementController> logger;
        private readonly IRegManagementService regManagementService;
       
        public RegisterManagementController(ILogger<RegisterManagementController> logger, IRegManagementService regManagementService)
        {
            this.logger = logger;
            this.regManagementService = regManagementService;           
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
            ProviderDetailsViewModel providerDetailsViewModel = new ProviderDetailsViewModel();
            providerDetailsViewModel.Provider = providerDto;
            providerDetailsViewModel.CertificateInformationList = new List<CertificateInformationDto>();
            providerDetailsViewModel.CertificateInformationList = providerDto.CertificateInformation
           .Where(x => x.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish).ToList();
            return View(providerDetailsViewModel);
        }
        [HttpGet("service-details")]
        public IActionResult ServiceDetails(int serviceId)
        {
            return View();
        }
        [HttpGet("publish-service")]
        public IActionResult PublishService()
        {
            return View();
        }
        [HttpGet("what-next")]
        public IActionResult WhatNext()
        {
            return View();
        }
        [HttpGet("provider-published")]
        public IActionResult ProviderPublished()
        {
            return View();
        }
    }
}
