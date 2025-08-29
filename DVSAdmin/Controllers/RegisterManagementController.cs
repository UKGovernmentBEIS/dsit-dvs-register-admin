using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSAdmin.Models.CabTransfer;
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
        public async Task<IActionResult> ServiceDetails(int serviceKey,int pageNumber =1,  string currentSort = "status", string currentSortAction = "ascending")
        {
            ViewBag.CurrentSort = currentSort;
            ViewBag.CurrentSortAction = currentSortAction;
            ViewBag.CurrentPage = pageNumber;
            ServiceVersionViewModel serviceVersions = new();
            var serviceList = await regManagementService.GetServiceVersionList(serviceKey);
            ServiceDto currentServiceVersion = serviceList.OrderByDescending(x => x.ModifiedTime).FirstOrDefault() ?? new ServiceDto(); //Latest submission has latest date          
            SetServiceDataToSession(currentServiceVersion);
            
            serviceVersions.ServiceHistoryVersions = serviceList.Where(x => x != currentServiceVersion).ToList() ?? new List<ServiceDto>();
            serviceVersions.CurrentServiceVersion = currentServiceVersion;
            serviceVersions.CanResendRemovalRequest = currentServiceVersion.ServiceStatus == ServiceStatusEnum.AwaitingRemovalConfirmation && currentServiceVersion.ServiceRemovalReason != 0;
            serviceVersions.CanResendUpdateRequest = currentServiceVersion.ServiceStatus == ServiceStatusEnum.UpdatesRequested && currentServiceVersion.ServiceDraft != null
            && currentServiceVersion.Provider.ProviderProfileDraft == null;
            
            return View(serviceVersions);
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
            TFVersionViewModel TFVersionViewModel = new()
            {
                SelectedTFVersion = null
            };
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
            SelectCabViewModel selectCabViewModel = new()
            {
                SelectedCabId = null,
                SelectedCabName = null
            };
            List<SchemeIdentityProfileMappingViewModel> schemeIdentityProfileMappingViewModelList = [];
            List<SchemeQualityLevelMappingViewModel> schemeQualityLevelMappingViewModelList = [];

            if (serviceDto.TrustFrameworkVersion != null)
            {
                TFVersionViewModel.SelectedTFVersion = serviceDto.TrustFrameworkVersion;
            }

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
                foreach (var item in serviceDto.ServiceSupSchemeMapping)
                {

                    if (item.SchemeGPG45Mapping != null && item.SchemeGPG45Mapping.Count > 0)
                    {
                        SchemeIdentityProfileMappingViewModel schemeIdentityProfileMappingViewModel = new();
                        schemeIdentityProfileMappingViewModel.SchemeId = item.SupplementarySchemeId;
                        schemeIdentityProfileMappingViewModel.IdentityProfile = new IdentityProfileViewModel
                        {
                            SelectedIdentityProfiles = item.SchemeGPG45Mapping.Select(mapping => mapping.IdentityProfile).ToList()
                        };

                        schemeIdentityProfileMappingViewModelList.Add(schemeIdentityProfileMappingViewModel);
                    }

                    if (item.HasGpg44Mapping != null)
                    {
                        SchemeQualityLevelMappingViewModel schemeQualityLevelMappingViewModel = new();
                        schemeQualityLevelMappingViewModel.HasGPG44 = item.HasGpg44Mapping;
                        schemeQualityLevelMappingViewModel.SchemeId = item.SupplementarySchemeId;
                        if (item.SchemeGPG44Mapping != null && item.SchemeGPG44Mapping.Count > 0)
                        {

                            schemeQualityLevelMappingViewModel.QualityLevel = new QualityLevelViewModel
                            {
                                SelectedQualityofAuthenticators = item.SchemeGPG44Mapping.Select(mapping => mapping.QualityLevel).Where(x => x.QualityType == QualityTypeEnum.Authentication).ToList(),
                                SelectedLevelOfProtections = item.SchemeGPG44Mapping.Select(mapping => mapping.QualityLevel).Where(x => x.QualityType == QualityTypeEnum.Protection).ToList(),
                            };
                        }
                        schemeQualityLevelMappingViewModelList.Add(schemeQualityLevelMappingViewModel);
                    }

                }
            }
            if (serviceDto?.ManualUnderPinningService?.Cab != null)
            {
                selectCabViewModel.SelectedCabId = serviceDto.ManualUnderPinningService.Cab.Id;
                selectCabViewModel.SelectedCabName = serviceDto.ManualUnderPinningService.Cab.CabName;
            }
            else if (serviceDto?.UnderPinningService != null)
            {
                selectCabViewModel.SelectedCabId = serviceDto.UnderPinningService.CabUser.Cab.Id;
                selectCabViewModel.SelectedCabName = serviceDto.UnderPinningService.CabUser.Cab.CabName;
            }



            ServiceSummaryViewModel serviceSummary = new()
            {
                TFVersionViewModel = TFVersionViewModel,
                ServiceName = serviceDto.ServiceName,
                ServiceURL = serviceDto.WebSiteAddress,
                CompanyAddress = serviceDto.CompanyAddress,
                RoleViewModel = roleViewModel,
                IdentityProfileViewModel = identityProfileViewModel,
                QualityLevelViewModel = qualityLevelViewModel,             
                HasSupplementarySchemes = serviceDto.HasSupplementarySchemes,

                ServiceType = serviceDto?.ServiceType ?? 0,
                SelectCabViewModel = selectCabViewModel,
                IsUnderpinningServicePublished = serviceDto?.IsUnderPinningServicePublished,
               
                SelectedManualUnderPinningServiceId = serviceDto?.ManualUnderPinningServiceId,//non published manual               
                SelectedUnderPinningServiceId = serviceDto?.UnderPinningServiceId,// published

                UnderPinningServiceName = serviceDto?.UnderPinningServiceId == null ? serviceDto?.ManualUnderPinningService?.ServiceName : serviceDto?.UnderPinningService?.ServiceName,
                UnderPinningProviderName = serviceDto?.UnderPinningServiceId == null ? serviceDto?.ManualUnderPinningService?.ProviderName : serviceDto?.UnderPinningService.Provider?.RegisteredName,
                UnderPinningServiceExpiryDate = serviceDto?.UnderPinningServiceId == null ? serviceDto?.ManualUnderPinningService?.CertificateExpiryDate : serviceDto?.UnderPinningService.ConformityExpiryDate,
                CabId = serviceDto?.UnderPinningServiceId == null ? serviceDto?.ManualUnderPinningService?.Cab.Id??0 : serviceDto?.UnderPinningService.CabUser.CabId??0,

                HasGPG44 = serviceDto.HasGPG44,
                HasGPG45 = serviceDto.HasGPG45,
                FileName = serviceDto.FileName,
                SupplementarySchemeViewModel = supplementarySchemeViewModel,
                SchemeIdentityProfileMapping = schemeIdentityProfileMappingViewModelList,
                SchemeQualityLevelMapping = schemeQualityLevelMappingViewModelList,

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
