using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{

    [Route("remove")]
    //Methods/Actions/Views for publishing services
    //Session is used only in PublishService method to keep published service ids
    //as there are no user input fields in other methods
    //Any change in the controller routes to be verified
    //with button or ahref actions in .cshtml
    public class RemoveProviderController : BaseController
    {
        private readonly IRemoveProviderService removeProviderService;
        private readonly IRegManagementService regManagementService;
        private readonly IUserService userService;
      
        public RemoveProviderController(IRemoveProviderService removeProviderService, IUserService userService, IRegManagementService regManagementService)
        {
            this.removeProviderService = removeProviderService;
            this.userService = userService;
            this.regManagementService = regManagementService;
        }   



        #region Remove Provider

        [HttpGet("provider/reason-for-removal")]
        public IActionResult ReasonForRemoval(int providerId, RemovalReasonsEnum? removalReason)
        {
            ProviderRemovalViewModel providerRemovalViewModel = new() { ProviderId = providerId };
            if (removalReason.HasValue)
            {
                providerRemovalViewModel.RemovalReason = removalReason.Value;
            }
            return View(providerRemovalViewModel);
        }

        [HttpPost("provider/proceed-with-removal")]
        public async Task<IActionResult> ProceedWithRemoval(ProviderRemovalViewModel providerRemovalViewModel)
        {
            if (ModelState.IsValid)
            {
                ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerRemovalViewModel.ProviderId);
                var userEmails = await userService.GetUserEmailsExcludingLoggedIn(UserEmail);
                providerProfileDto.DSITUserEmails = string.Join(",", userEmails);
                providerProfileDto.RemovalReason = providerRemovalViewModel.RemovalReason;
                return View("ProceedRemoval", providerProfileDto);
                
            }
            else
            {
                return View("ReasonForRemoval", providerRemovalViewModel);
            }           
        }


        [HttpPost("provider/publish-removal-reason")]
        public async Task<IActionResult> RequestProviderRemoval(ProviderProfileDto providerDetailsViewModel, RemovalReasonsEnum removalReason)
        {
            List<string> dsitUserEmails = providerDetailsViewModel.DSITUserEmails.Split(',').ToList();
            ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerDetailsViewModel.Id);
            List<int> ServiceIds = providerProfileDto.Services
               .Where(item => item.ServiceStatus == ServiceStatusEnum.Published || item.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation)
               .Select(item => item.Id)
               .ToList();
            GenericResponse genericResponse = await removeProviderService.RemoveProviderRequest(providerProfileDto.Id, ServiceIds, UserEmail, dsitUserEmails, removalReason);

            if (genericResponse.Success)
            {
                return RedirectToAction("RemovalConfirmation", new { providerId = providerProfileDto.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the removal reason.");
                return View("ReasonForRemoval", providerProfileDto);
            }
        }



        [HttpGet("provider/removal-confirmation")]
        public async Task<IActionResult> RemovalConfirmation(int providerId)
        {
            ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerId);
            return View(providerProfileDto);
        }

        #endregion

        #region Remove Service
        [HttpGet("service/service-removal-reason")]
        public IActionResult ServiceRemovalReason(int providerId, int serviceId, ServiceRemovalReasonEnum serviceRemovalReason)
        {
            ServiceRemovalViewModel serviceRemovalViewModel = new ()
            {
                ProviderId = providerId,
                ServiceId = serviceId,
                ServiceRemovalReason = serviceRemovalReason
            };
            return View(serviceRemovalViewModel);
        }

        [HttpPost("service/proceed-with-service-removal")]
        public async Task<IActionResult> ProceedWithServiceRemoval(ServiceRemovalViewModel serviceRemovalViewModel)
        {
            if (ModelState.IsValid)
            {
                ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceRemovalViewModel.ServiceId);
                serviceDto.ServiceRemovalReason = serviceRemovalViewModel.ServiceRemovalReason;
                return View("ProceedServiceRemoval", serviceDto);
            }
            else
            {
                return View("ServiceRemovalReason", serviceRemovalViewModel);
            }           
        }

        [HttpPost("service/publish-service-removal-reason")]
        public async Task<IActionResult> RequestServiceRemoval(ServiceDto serviceDetailsViewModel, ServiceRemovalReasonEnum serviceRemovalReason)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceDetailsViewModel.Id);
            List<int> ServiceIds = [serviceDto.Id];
            GenericResponse genericResponse = await removeProviderService.RemoveServiceRequest(serviceDetailsViewModel.ProviderProfileId, ServiceIds, UserEmail, serviceRemovalReason);

            if (genericResponse.Success)
            {
                return RedirectToAction("ServiceRemovalConfirmation", new { providerId = serviceDetailsViewModel.ProviderProfileId, serviceId = serviceDto.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the removal reason.");
                return View("ServiceRemovalReason", new { providerId = serviceDetailsViewModel.ProviderProfileId, serviceId = serviceDto.Id });
            }
        }

        [HttpGet("service/service-removal-confirmation")]
        public async Task<IActionResult> ServiceRemovalConfirmation(int providerId, int serviceId)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceId);
            ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerId);
            serviceDto.Provider = providerProfileDto;
            return View(serviceDto);
        }

        #endregion

        #region Cancel Removal

        [HttpGet("service/cancel-service-removal")]
        public async Task<IActionResult> CancelRemoval(int serviceId)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceId);
            return View(serviceDto);
        }

        [HttpPost("service/proceed-with-canceling-service-removal")]
        public async Task<IActionResult> ProceedRemovalCancellation(int serviceId, int providerId)
        {
            GenericResponse genericResponse = await removeProviderService.CancelRemoveServiceRequest(providerId, serviceId, UserEmail); 
            if (genericResponse.Success)
            {
                ViewBag.providerId = providerId;
                return View("CancelRemovalConfirmation");
            }
            else
            {
                return RedirectToAction("CancelRemoval", new { serviceId });
            }           
        }

        #endregion

        #region  Resend Removal Request
        [HttpPost("service/resend-removal-request")]
        public async Task<ActionResult> ResendRemovalRequest(int serviceId)
        {
            GenericResponse genericResponse = new GenericResponse();

            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceId);

            List<int> serviceIds = [serviceDto.Id];
            genericResponse = await removeProviderService.GenerateTokenAndSendServiceRemoval(serviceDto.ProviderProfileId, serviceIds, UserEmail, serviceDto.ServiceRemovalReason, true);
            ViewBag.providerId = serviceDto.ProviderProfileId;

            if (genericResponse.Success)
            {
                return View("ResendRemovalEmailConformation");
            }
            return RedirectToAction("ServiceDetails", "RegisterManagement", new { serviceKey = serviceDto.ServiceKey });
        }
        #endregion

        #region removal requested by cab

        [HttpGet("cab-removal")]
        public async Task<IActionResult> CabRemoval(int providerId, int serviceId, string whatToRemove)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceId);
            ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerId);
            ViewBag.whatToRemove = whatToRemove;
            serviceDto.Provider = providerProfileDto;
            List<int> ServiceIds = [serviceId];
            List<string> activeCabEmails = await regManagementService.GetCabEmailListForServices(ServiceIds);


            var cabRemovalViewModel = new CabRemovalViewModel
            {
                Service = serviceDto,
                WhatToRemove = whatToRemove,
                ActiveCabEmails = string.Join(",", activeCabEmails)

             };
        
            return View("CabRemoval", cabRemovalViewModel);
        }

        [HttpPost("cab-publish-removal")]
        public async Task<IActionResult> SubmitRemoval(CabRemovalViewModel cabRemovalViewModel)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(cabRemovalViewModel.Service.Id);           
            List<int> ServiceIds = [serviceDto.Id];
            List<string> activeCabEmails = cabRemovalViewModel.ActiveCabEmails.Split(',').ToList();
            GenericResponse genericResponse = await removeProviderService.RemoveServiceRequestByCab(cabRemovalViewModel.Service.ProviderProfileId, ServiceIds, UserEmail, activeCabEmails, null);
         
            if (genericResponse.Success)
            {
                return RedirectToAction("CabRemovalConfirmation", new { providerId = cabRemovalViewModel.Service.ProviderProfileId, serviceId = cabRemovalViewModel.Service.Id, whatToRemove = cabRemovalViewModel.WhatToRemove });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the removal reason.");
                return View("CabRemoval", serviceDto);
            }
        }


        [HttpGet("cab-removal-confirmation")]
        public async Task<IActionResult> CabRemovalConfirmation(int providerId, int serviceId, string whatToRemove)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceId);
            ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerId);
            ViewBag.whatToRemove = whatToRemove;
            serviceDto.Provider = providerProfileDto;
            return View(serviceDto);
        }
        #endregion
    }
}
