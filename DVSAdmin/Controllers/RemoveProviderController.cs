using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("remove-provider")]
    //Methods/Actions/Views for publishing services
    //Session is used only in PublishService method to keep published service ids
    //as there are no user input fields in other methods
    //Any change in the controller routes to be verified
    //with button or ahref actions in .cshtml
    public class RemoveProviderController : Controller
    {
        private readonly IRemoveProviderService removeProviderService;

        private string userEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;
        public RemoveProviderController(IRemoveProviderService removeProviderService)
        {
            this.removeProviderService = removeProviderService;

        }

        [HttpGet("cab-removal")]
        public async Task<IActionResult> CabRemoval(int providerId, int serviceId, string whatToRemove)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(serviceId);
            ProviderProfileDto providerProfileDto = await removeProviderService.GetProviderDetails(providerId);
            ViewBag.whatToRemove = whatToRemove;

            serviceDto.Provider = providerProfileDto;

            return View("CabRemoval", serviceDto);
        }

        [HttpPost("cab-publish-removal")]
        public async Task<IActionResult> SubmitRemoval(CabRemovalViewModel cabRemovalViewModel)
        {
            ServiceDto serviceDto = await removeProviderService.GetServiceDetails(cabRemovalViewModel.Service.Id);
            List<int> ServiceIds = [serviceDto.Id];
            GenericResponse genericResponse = await removeProviderService.RemoveServiceRequestByCab(cabRemovalViewModel.Service.ProviderProfileId, ServiceIds, userEmail, null);
 
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

    }
}
