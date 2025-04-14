using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSAdmin.Models.RegManagement;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;


namespace DVSAdmin.Controllers
{

    [Route("register-management")]
    //Methods/Actions/Views for publishing services
    //Session is used only in PublishService method to keep published service ids
    //as there are no user input fields in other methods
    //Any change in the controller routes to be verified
    //with button or ahref actions in .cshtml
    public class RegisterManagementController : BaseController
    {       
        private readonly IRegManagementService regManagementService;  
        private readonly IBucketService bucketService;
        private readonly ICsvDownloadService csvDownloadService;
              
        public RegisterManagementController(
            IRegManagementService regManagementService,            
            IBucketService bucketService,
            ICsvDownloadService csvDownloadService)
        {
            this.regManagementService = regManagementService;               
            this.bucketService = bucketService;
            this.csvDownloadService = csvDownloadService;
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
            SetServiceDataToSession(currentServiceVersion);
            
            serviceVersions.ServiceHistoryVersions = serviceList.Where(x => x != currentServiceVersion).ToList() ?? new List<ServiceDto>();
            serviceVersions.CurrentServiceVersion = currentServiceVersion;
            serviceVersions.CanResendRemovalRequest = currentServiceVersion.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation && currentServiceVersion.ServiceRemovalReason != 0;

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
                if (serviceids == null && !serviceids.Any())
                    throw new InvalidOperationException("No service IDs found in session to publish.");
                
                GenericResponse genericResponse = await regManagementService.UpdateServiceStatus(serviceids, providerDetailsViewModel.Id,UserEmail);
                if (genericResponse.Success)
                {
                    return RedirectToAction("ProviderPublished", new { providerId  = providerDetailsViewModel.Id });
                }
                else
                {
                    throw new InvalidOperationException("Failed to update service status during publication.");
                }
            }
            else if(action == "cancel")
            {
                return RedirectToAction("ProceedPublication", new { providerId = providerDetailsViewModel.Id });
            }
            else
            {
                throw new InvalidOperationException("Invalid action received during service publication.");
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
                    throw new InvalidOperationException($"Failed to download certificate: Empty or null content for key.");

                string contentType = "application/octet-stream";
                return File(fileContent, contentType, filename);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Exception occurred while downloading certificate with key.", ex);
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
                throw new InvalidOperationException("No data available for download", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while attempting to download the register.", ex);
            }
        }

        #region private methods
        private void SetServiceDataToSession(ServiceDto serviceDto)
        {
            RoleViewModel roleViewModel = new()
            {
                SelectedRoles = [],
            };
            QualityLevelViewModel qualityLevelViewModel = new()
            {
                SelectedLevelOfProtections = [],
                SelectedQualityofAuthenticators = []
            };

            IdentityProfileViewModel identityProfileViewModel = new()
            {
                SelectedIdentityProfiles = []
            };

            SupplementarySchemeViewModel supplementarySchemeViewModel = new()
            {
                SelectedSupplementarySchemes = []
            };

            if (serviceDto.ServiceRoleMapping != null && serviceDto.ServiceRoleMapping.Count > 0)
            {
                roleViewModel.SelectedRoles = serviceDto.ServiceRoleMapping.Select(mapping => mapping.Role).ToList();
            }

            if (serviceDto.ServiceQualityLevelMapping != null && serviceDto.ServiceQualityLevelMapping.Count > 0)
            {
                var protectionLevels = serviceDto.ServiceQualityLevelMapping
                    .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Protection)
                    .ToList();

                foreach (var item in protectionLevels)
                {
                    qualityLevelViewModel.SelectedLevelOfProtections.Add(item.QualityLevel);
                }

                var authenticatorLevels = serviceDto.ServiceQualityLevelMapping
                    .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                    .ToList();

                foreach (var item in authenticatorLevels)
                {
                    qualityLevelViewModel.SelectedQualityofAuthenticators.Add(item.QualityLevel);
                }
            }

            if (serviceDto.ServiceIdentityProfileMapping != null && serviceDto.ServiceIdentityProfileMapping.Count > 0)
            {
                identityProfileViewModel.SelectedIdentityProfiles = serviceDto.ServiceIdentityProfileMapping.Select(mapping => mapping.IdentityProfile).ToList();
            }
            if (serviceDto.ServiceSupSchemeMapping != null && serviceDto.ServiceSupSchemeMapping.Count > 0)
            {
                supplementarySchemeViewModel.SelectedSupplementarySchemes = serviceDto.ServiceSupSchemeMapping.Select(mapping => mapping.SupplementaryScheme).ToList();
            }


            ServiceSummaryViewModel serviceSummary = new()
            {
                ServiceName = serviceDto.ServiceName,
                ServiceURL = serviceDto.WebSiteAddress,
                CompanyAddress = serviceDto.CompanyAddress,
                RoleViewModel = roleViewModel,
                IdentityProfileViewModel = identityProfileViewModel,
                QualityLevelViewModel = qualityLevelViewModel,
                HasSupplementarySchemes = serviceDto.HasSupplementarySchemes,
                HasGPG44 = serviceDto.HasGPG44,
                HasGPG45 = serviceDto.HasGPG45,
                FileName = serviceDto.FileName,
                SupplementarySchemeViewModel = supplementarySchemeViewModel,
                ConformityIssueDate = serviceDto.ConformityIssueDate == DateTime.MinValue ? null : serviceDto.ConformityIssueDate,
                ConformityExpiryDate = serviceDto.ConformityExpiryDate == DateTime.MinValue ? null : serviceDto.ConformityExpiryDate,
                ServiceId = serviceDto.Id,
                Provider = serviceDto.Provider,
                CabUserId = serviceDto.CabUserId,
                ServiceKey = serviceDto.ServiceKey
            };
            HttpContext?.Session.Set("ServiceSummary", serviceSummary);
        }
        #endregion
    }
}
