﻿using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
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
        private readonly IBucketService bucketService;
        private readonly ICsvDownloadService csvDownloadService;
        private readonly ILogger<RegisterManagementController> logger;
        private string userEmail => HttpContext.Session.Get<string>("Email")??string.Empty;       
        public RegisterManagementController(
            IRegManagementService regManagementService,            
            IBucketService bucketService,
            ICsvDownloadService csvDownloadService,
            ILogger<RegisterManagementController> logger)
        {
           
            this.regManagementService = regManagementService;               
            this.bucketService = bucketService;
            this.csvDownloadService = csvDownloadService;
            this.logger = logger;
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
        public async Task<IActionResult> ServiceDetails(int serviceKey)
        {
            ServiceVersionViewModel serviceVersions = new();
            var serviceList = await regManagementService.GetServiceVersionList(serviceKey);
            ServiceDto currentServiceVersion = serviceList.OrderByDescending(x => x.ModifiedTime).FirstOrDefault() ?? new ServiceDto(); //Latest submission has latest date
            serviceVersions.ServiceHistoryVersions = serviceList.Where(x => x != currentServiceVersion).ToList() ?? new List<ServiceDto>();
            serviceVersions.CurrentServiceVersion = currentServiceVersion;

            return View(serviceVersions);
        }

        [HttpGet("publish-service")]
        public async Task<IActionResult> PublishService(int providerId)
        {           
            ProviderProfileDto providerDto = await regManagementService.GetProviderWithServiceDetails(providerId);
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

        [HttpGet("download-register")]
        public async Task<IActionResult> DownloadRegister()
        {
            try
            {
                var result = await csvDownloadService.DownloadAsync();
                
                return File(
                    result.FileContent, 
                    result.ContentType, 
                    result.FileName
                );
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "No data available for download");
                return RedirectToAction(Constants.ErrorPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Download failed");
                return RedirectToAction(Constants.ErrorPath);
            }
        }

    }
}
