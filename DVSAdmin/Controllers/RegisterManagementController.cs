using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models.RegManagement;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
 
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
        private readonly IRegManagementService regManagementService;
        private readonly ICertificateReviewService certificateReviewService;
        private readonly IBucketService bucketService;
        private string userEmail => HttpContext.Session.Get<string>("Email")??string.Empty;
        public RegisterManagementController(IRegManagementService regManagementService, ICertificateReviewService certificateReviewService, IBucketService bucketService)
        {
           
            this.regManagementService = regManagementService;
            this.certificateReviewService = certificateReviewService;
            this.bucketService = bucketService;
        }

        [HttpGet("register-management-list")]
        public async Task<IActionResult> RegisterManagement()
        {
            var providersList = await regManagementService.GetProviders();
            var providerListViewModel = new ProviderListViewModel
            {
                AllStatusesList = providersList.ToList()
            };
            return View(providerListViewModel);
        }

        [HttpGet("provider-details")]
        public async Task<IActionResult> ProviderDetails(int providerId)
        {
            ProviderProfileDto providerDto = await regManagementService.GetProviderDetails(providerId);
            return View(providerDto);
        }

        [HttpGet("service-details")]
        public async Task<IActionResult> ServiceDetails(int serviceId)
        {
            ServiceDto serviceDto = await certificateReviewService.GetServiceDetails(serviceId);
            return View(serviceDto);
        }

        
        [HttpGet("publish-service")]
        public async Task<IActionResult> PublishService(int providerId)
        {           
            ProviderProfileDto providerDto = await regManagementService.GetProviderWithServiceDeatils(providerId);
            providerDto.Services= providerDto.Services.Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish).ToList();
            List<int> ServiceIds = providerDto.Services.Select(item => item.Id).ToList();
            HttpContext?.Session.Set("ServiceIdsToPublish", ServiceIds);              
            return View(providerDto);
        }


        [HttpGet("proceed-publication")]
        public async Task<IActionResult> ProceedPublication(int providerId)
        {
            //To make sure only the service ids reviewed in previous screen is fetched
            List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish") ?? new List<int>();
            ProviderProfileDto providerProfileDto = await regManagementService.GetProviderDetails(providerId);
            providerProfileDto.Services =  providerProfileDto.Services.Where(item => serviceids.Contains(item.Id)).ToList();          
            return View(providerProfileDto);
        }

        [HttpGet("about-to-publish")]
        public ActionResult AboutToPublish(int providerId)
        {
         ProviderProfileDto providerProfileDto = new ProviderProfileDto { Id = providerId }; 
         return View(providerProfileDto);
        }

        [HttpPost("about-to-publish")]
        public async Task<IActionResult> Publish(ProviderProfileDto providerDetailsViewModel, string action)
        {
            if(action == "publish")
            {
              
                List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish") ?? new List<int>();
                if (serviceids != null && serviceids.Any())
                {
                    GenericResponse genericResponse = await regManagementService.UpdateServiceStatus(serviceids, providerDetailsViewModel.Id,userEmail);
                    if (genericResponse.Success)
                    {
                        return RedirectToAction("ProviderPublished", new { providerId  = providerDetailsViewModel.Id });

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
                return RedirectToAction("ProceedPublication", new { providerId = providerDetailsViewModel.Id });
            }
            else
            {
                return RedirectToAction(Constants.ErrorPath);
            }           
         
        }

        [HttpGet("provider-published")]
        public async Task<IActionResult> ProviderPublished(int providerId)
        {
            ProviderProfileDto providerProfileDto = await regManagementService.GetProviderDetails(providerId);
            List<int> serviceids = HttpContext?.Session.Get<List<int>>("ServiceIdsToPublish") ?? new List<int>();
            providerProfileDto.Services = providerProfileDto.Services.Where(x => serviceids.Contains(x.Id)).ToList();
            HttpContext.Session.Remove("ServiceIdsToPublish");
            return View(providerProfileDto);
        }

        [HttpGet("reason-for-removal")]
        public async Task<IActionResult> ReasonForRemoval(int providerId)
        {
            ProviderProfileDto providerDto = await regManagementService.GetProviderDetails(providerId);
            return View(providerDto);
        }

        [HttpPost("publish-removal-reason")]
        public async Task<IActionResult> PublishRemovalReason(ProviderProfileDto providerDetailsViewModel, RemovalReasonsEnum removalReason)
        {
            ProviderProfileDto providerProfileDto = await regManagementService.GetProviderDetails(providerDetailsViewModel.Id);
            List<int> ServiceIds = providerProfileDto.Services.Select(item => item.Id).ToList();
            GenericResponse genericResponse = await regManagementService.UpdateRemovalStatus(EventTypeEnum.RemoveProvider, removalReason, providerProfileDto.Id, ServiceIds, User.Identity.Name);

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

        [HttpPost("proceed-with-removal")]
        public async Task<IActionResult> ProceedWithRemoval(int providerId, RemovalReasonsEnum? removalReason)
        {
            ProviderProfileDto providerProfileDto = await regManagementService.GetProviderDetails(providerId);

            if (removalReason == null)
            {
                ModelState.AddModelError("RemovalReason", "Select a reason for removal");
            }           
            if (!ModelState.IsValid)
            {
                return View("ReasonForRemoval", providerProfileDto);
            }

            providerProfileDto.RemovalReason = removalReason.Value;
            return View("ProceedRemoval", providerProfileDto);
        }



        [HttpGet("removal-confirmation")]
        public async Task<IActionResult> RemovalConfirmation(int providerId)
        {
            ProviderProfileDto providerProfileDto = await regManagementService.GetProviderDetails(providerId);
            return View(providerProfileDto);
        }








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
                    return RedirectToAction(Constants.ErrorPath);
                }
                string contentType = "application/octet-stream";
                return File(fileContent, contentType, filename);
            }
            catch (Exception)
            {
                return RedirectToAction(Constants.ErrorPath);
            }
        }

    }
}
