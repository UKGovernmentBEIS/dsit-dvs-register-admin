using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("edit-service")]
    [ValidCognitoToken]
    public class EditServiceController : Controller
    {
        private readonly IEditService editService;
        private readonly IBucketService bucketService;
        private readonly IUserService userService;
        public EditServiceController(IEditService editService, IBucketService bucketService, IUserService userService)
        {
            this.editService = editService;
            this.bucketService = bucketService;
            this.userService = userService;

        }

           
        #region Service Name
            [HttpGet("name-of-service")]
        public async Task<IActionResult> ServiceName(bool fromSummaryPage, int serviceId)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();
            HttpContext?.Session.Set("ServiceSummary", serviceSummaryViewModel);
            return View(serviceSummaryViewModel);

        }

        #endregion
        #region Company Address
        [HttpGet("company-address")]
        public async Task<IActionResult> CompanyAddress(bool fromSummaryPage, int serviceId)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;

            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();

            HttpContext?.Session.Set("ServiceSummary", serviceSummaryViewModel);
            return View(serviceSummaryViewModel);
        }

        [HttpPost("company-address")]
        public IActionResult CompanyAddress(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            if (ModelState["CompanyAddress"].Errors.Count == 0)
            {
                ServiceSummaryViewModel serviceSummary = GetServiceSummary();
                serviceSummary.CompanyAddress = serviceSummaryViewModel.CompanyAddress;
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return RedirectToAction("ServiceSummary");
            }
           return View(serviceSummaryViewModel);
        }
        #endregion
        #region Summary and save to database
        [HttpGet("check-your-answers")]
        public IActionResult ServiceSummary()
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            return View(summaryViewModel);
        }

        [HttpGet("summary-of-changes")]
        public async Task<IActionResult> ServiceDifference()
        {
            ServiceChangesViewModel changesViewModel = new();
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            ServiceDto serviceDto = await editService.GetService(summaryViewModel.ServiceId);
            ServiceDraftDto serviceDraftDto = CreateDraft(serviceDto, summaryViewModel);
            changesViewModel.CurrentService = serviceDto;
            changesViewModel.ChangedService = serviceDraftDto;
            return View(changesViewModel);
        }


        /// <summary>
        ///Final page if save success
        /// </summary>       
        /// <returns></returns>
        [HttpGet("service-submitted")]
        public async Task<IActionResult> InformationSubmitted()
        {
            string email = HttpContext?.Session.Get<string>("Email") ?? string.Empty;
            HttpContext?.Session.Remove("ServiceSummary");
            return View();

        }

        #endregion
        #region Private Methods
        private ServiceSummaryViewModel GetServiceSummary()
        {
            ServiceSummaryViewModel model = HttpContext?.Session.Get<ServiceSummaryViewModel>("ServiceSummary") ?? new ServiceSummaryViewModel
            {
                QualityLevelViewModel = new QualityLevelViewModel { SelectedLevelOfProtections = new List<QualityLevelDto>(), SelectedQualityofAuthenticators = new List<QualityLevelDto>() },
                RoleViewModel = new RoleViewModel { SelectedRoles = new List<RoleDto>() },
                IdentityProfileViewModel = new IdentityProfileViewModel { SelectedIdentityProfiles = new List<IdentityProfileDto>() },
                SupplementarySchemeViewModel = new SupplementarySchemeViewModel { SelectedSupplementarySchemes = new List<SupplementarySchemeDto> { } }
            };
            return model;
        }

        private ServiceDto MapViewModelToDto(ServiceSummaryViewModel model)
        {
            ServiceDto serviceDto = null;
            if (model != null)
            {
                serviceDto = new();
                ICollection<ServiceQualityLevelMappingDto> serviceQualityLevelMappings = new List<ServiceQualityLevelMappingDto>();
                ICollection<ServiceRoleMappingDto> serviceRoleMappings = new List<ServiceRoleMappingDto>();
                ICollection<ServiceIdentityProfileMappingDto> serviceIdentityProfileMappings = new List<ServiceIdentityProfileMappingDto>();
                ICollection<ServiceSupSchemeMappingDto> serviceSupSchemeMappings = new List<ServiceSupSchemeMappingDto>();

                foreach (var item in model.QualityLevelViewModel.SelectedQualityofAuthenticators)
                {
                    serviceQualityLevelMappings.Add(new ServiceQualityLevelMappingDto { QualityLevelId = item.Id });
                }
                foreach (var item in model.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    serviceQualityLevelMappings.Add(new ServiceQualityLevelMappingDto { QualityLevelId = item.Id });
                }
                foreach (var item in model.RoleViewModel.SelectedRoles)
                {
                    serviceRoleMappings.Add(new ServiceRoleMappingDto { RoleId = item.Id });
                }
                foreach (var item in model.IdentityProfileViewModel.SelectedIdentityProfiles)
                {
                    serviceIdentityProfileMappings.Add(new ServiceIdentityProfileMappingDto { IdentityProfileId = item.Id });
                }
                foreach (var item in model.SupplementarySchemeViewModel.SelectedSupplementarySchemes)
                {
                    serviceSupSchemeMappings.Add(new ServiceSupSchemeMappingDto { SupplementarySchemeId = item.Id });
                }

                serviceDto.Provider = model.Provider;
                serviceDto.ServiceName = model.ServiceName;
                serviceDto.WebSiteAddress = model.ServiceURL;
                serviceDto.CompanyAddress = model.CompanyAddress;
                serviceDto.ServiceRoleMapping = serviceRoleMappings;
                serviceDto.ServiceIdentityProfileMapping = serviceIdentityProfileMappings;
                serviceDto.ServiceQualityLevelMapping = serviceQualityLevelMappings;
                serviceDto.HasSupplementarySchemes = model.HasSupplementarySchemes;
                serviceDto.HasGPG44 = model.HasGPG44;
                serviceDto.HasGPG45 = model.HasGPG45;
                serviceDto.ServiceSupSchemeMapping = serviceSupSchemeMappings;
                serviceDto.FileLink = model.FileLink;
                serviceDto.FileName = model.FileName;
                serviceDto.FileSizeInKb = model.FileSizeInKb;
                serviceDto.ConformityIssueDate = Convert.ToDateTime(model.ConformityIssueDate);
                serviceDto.ConformityExpiryDate = Convert.ToDateTime(model.ConformityExpiryDate);
                serviceDto.CabUserId = model.CabUserId;
                serviceDto.Id = model.ServiceId;
                serviceDto.ServiceKey = model.ServiceKey;
            }
            return serviceDto;


        }

        private void SetServiceDataToSession(ServiceDto serviceDto)
        {
            RoleViewModel roleViewModel = new()
            {
                SelectedRoles = []
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
                .Select(item => item.QualityLevel);

                var authenticatorLevels = serviceDto.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                .Select(item => item.QualityLevel);

                foreach (var item in protectionLevels)
                {
                    qualityLevelViewModel.SelectedLevelOfProtections.Add(item);
                }

                foreach (var item in authenticatorLevels)
                {
                    qualityLevelViewModel.SelectedQualityofAuthenticators.Add(item);
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
                SupplementarySchemeViewModel = supplementarySchemeViewModel,
                FileLink = serviceDto.FileLink,
                FileName = serviceDto.FileName,
                FileSizeInKb = serviceDto.FileSizeInKb,
                ConformityIssueDate = serviceDto.ConformityIssueDate == DateTime.MinValue ? null : serviceDto.ConformityIssueDate,
                ConformityExpiryDate = serviceDto.ConformityExpiryDate == DateTime.MinValue ? null : serviceDto.ConformityExpiryDate,
                ServiceId = serviceDto.Id,
                Provider = serviceDto.Provider,
                CabUserId = serviceDto.CabUserId,
                ServiceKey = serviceDto.ServiceKey
            };
            HttpContext?.Session.Set("ServiceSummary", serviceSummary);
        }

        private ServiceDraftDto CreateDraft(ServiceDto existingService, ServiceSummaryViewModel updatedService)
        {
            var existingRoleIds = existingService.ServiceRoleMapping.Select(m => m.RoleId).ToList();
            var updatedRoleIds = updatedService.RoleViewModel.SelectedRoleIds;

            var existingProtectionIds = existingService.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Protection)
                .Select(item => item.QualityLevelId);
            var updatedProtectionIds = updatedService.QualityLevelViewModel.SelectedLevelOfProtectionIds;

            var existingAuthenticationIds = existingService.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                .Select(item => item.QualityLevelId);
            var updatedAuthenticationIds = updatedService.QualityLevelViewModel.SelectedQualityofAuthenticatorIds;

            var existingIdentityProfileIds = existingService.ServiceIdentityProfileMapping.Select(m => m.IdentityProfileId).ToList();
            var updatedIdentityProfileIds = updatedService.IdentityProfileViewModel.SelectedIdentityProfileIds;

            var existingSupSchemeIds = existingService.ServiceSupSchemeMapping.Select(m => m.SupplementarySchemeId).ToList();
            var updatedSupSchemeIds = updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemeIds;

            var draft = new ServiceDraftDto
            {
                serviceId = existingService.Id,
                PreviousServiceStatus = existingService.ServiceStatus,
                ProviderId = existingService.ProviderProfileId
            };

            ICollection<ServiceRoleMappingDraftDto> ServiceRoleMappingDraft  = new List<ServiceRoleMappingDraftDto>();
            ICollection<ServiceQualityLevelMappingDraftDto> ServiceQualityLevelMappingDraft = new List<ServiceQualityLevelMappingDraftDto>();
            ICollection<ServiceIdentityProfileMappingDraftDto> ServiceIdentityProfileMappingDraft = new List<ServiceIdentityProfileMappingDraftDto>();
            ICollection<ServiceSupSchemeMappingDraftDto> ServiceSupSchemeMappingDraft = new List<ServiceSupSchemeMappingDraftDto>();

            if (existingService.ServiceName != updatedService.ServiceName)
            {
                draft.ServiceName = updatedService.ServiceName;
            }

            if (existingService.CompanyAddress != updatedService.CompanyAddress)
            {
                draft.CompanyAddress = updatedService.CompanyAddress;
            }

            if (existingService.HasGPG44 != updatedService.HasGPG44)
            {
                draft.HasGPG44 = updatedService.HasGPG44;
            }

            if (existingService.HasGPG45 != updatedService.HasGPG45)
            {
                draft.HasGPG45 = updatedService.HasGPG45;
            }

            if (existingService.HasSupplementarySchemes != updatedService.HasSupplementarySchemes)
            {
                draft.HasSupplementarySchemes = updatedService.HasSupplementarySchemes;
            }

            if (existingService.ConformityIssueDate != updatedService.ConformityIssueDate)
            {
                draft.ConformityIssueDate = updatedService.ConformityIssueDate;
            }

            if (existingService.ConformityExpiryDate != updatedService.ConformityExpiryDate)
            {
                draft.ConformityExpiryDate = updatedService.ConformityExpiryDate;
            }

            if (!existingRoleIds.OrderBy(id => id).SequenceEqual(updatedRoleIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.RoleViewModel.SelectedRoles)
                {
                    ServiceRoleMappingDraft.Add(new ServiceRoleMappingDraftDto { RoleId = item.Id });
                }
            }
            if (!existingProtectionIds.OrderBy(id => id).SequenceEqual(updatedProtectionIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id });
                }
            }

            if (!existingAuthenticationIds.OrderBy(id => id).SequenceEqual(updatedAuthenticationIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedQualityofAuthenticators)
                {
                    ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id });
                }
            }

            if (!existingIdentityProfileIds.OrderBy(id => id).SequenceEqual(updatedIdentityProfileIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.IdentityProfileViewModel.SelectedIdentityProfiles)
                {
                    ServiceIdentityProfileMappingDraft.Add(new ServiceIdentityProfileMappingDraftDto { IdentityProfileId = item.Id });
                }
            }

            if (!existingSupSchemeIds.OrderBy(id => id).SequenceEqual(updatedSupSchemeIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id });
                }
            }

            return (draft);
        }
        #endregion
    }
}
