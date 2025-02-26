using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DVSAdmin.Controllers
{
    [Route("edit-service")]
    [ValidCognitoToken]
    public class EditServiceController : Controller
    {
        private readonly IEditService editService;
        private readonly IBucketService bucketService;
        private readonly IUserService userService;

        private string userEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;
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
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();

            serviceSummaryViewModel.FromSummaryPage = fromSummaryPage;
            return View(serviceSummaryViewModel);

        }

        [HttpPost("name-of-service")]
        public IActionResult ServiceName(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            if (ModelState["ServiceName"].Errors.Count == 0)
            {
                ServiceSummaryViewModel serviceSummary = GetServiceSummary();
                serviceSummary.ServiceName = serviceSummaryViewModel.ServiceName;
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return RedirectToAction("ServiceSummary");
            }
            return View(serviceSummaryViewModel);
        }
        #endregion

        #region Company Address
        [HttpGet("company-address")]
        public async Task<IActionResult> CompanyAddress(bool fromSummaryPage, int serviceId)
        {
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();

            serviceSummaryViewModel.FromSummaryPage = fromSummaryPage;
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

        #region Role
        [HttpGet("provider-roles")]
        public async Task<IActionResult> ProviderRoles(bool fromSummaryPage, int serviceId)
        {
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();
            RoleViewModel roleViewModel = new RoleViewModel();
            roleViewModel.SelectedRoleIds = serviceSummaryViewModel?.RoleViewModel?.SelectedRoles?.Select(c => c.Id).ToList();
            roleViewModel.AvailableRoles = await editService.GetRoles();

            roleViewModel.FromSummaryPage = fromSummaryPage;
            ViewBag.ServiceKey = serviceSummaryViewModel.ServiceKey;
            return View(roleViewModel);
        }

        [HttpPost("provider-roles")]
        public async Task<IActionResult> ProviderRoles(RoleViewModel roleViewModel)
        {
            bool fromSummaryPage = roleViewModel.FromSummaryPage;
            bool fromDetailsPage = roleViewModel.FromDetailsPage;
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            List<RoleDto> availableRoles = await editService.GetRoles();
            roleViewModel.AvailableRoles = availableRoles;
            roleViewModel.SelectedRoleIds = roleViewModel.SelectedRoleIds ?? new List<int>();
            if (roleViewModel.SelectedRoleIds.Count > 0)
                summaryViewModel.RoleViewModel.SelectedRoles = availableRoles.Where(c => roleViewModel.SelectedRoleIds.Contains(c.Id)).ToList();

            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return RedirectToAction("ServiceSummary");
            }
            else
            {
                return View("ProviderRoles", roleViewModel);
            }
        }

        #endregion

        #region GPG44 - input

        [HttpGet("gpg44-input")]
        public async Task<IActionResult> GPG44Input(bool fromSummaryPage, int serviceId)
        {
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

            return View(summaryViewModel);
        }

        [HttpPost("gpg44-input")]
        public async Task<IActionResult> GPG44Input(ServiceSummaryViewModel serviceSummaryViewModel)
        {            
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            if (ModelState["HasGPG44"].Errors.Count == 0)
            {
                serviceSummary.HasGPG44 = serviceSummaryViewModel.HasGPG44;


                if (Convert.ToBoolean(serviceSummary.HasGPG44))
                {
                    HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                    return RedirectToAction("GPG44");
                }
                else
                {
                    // clear selections if the value is changed from yes to no
                    serviceSummary.QualityLevelViewModel.SelectedQualityofAuthenticators = new List<QualityLevelDto>();
                    serviceSummary.QualityLevelViewModel.SelectedLevelOfProtections = new List<QualityLevelDto>();
                    HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                    return RedirectToAction("ServiceSummary");

                }
            }
            else
            {
                return View("GPG44Input", serviceSummary);
            }
        }
        #endregion

        #region GPG44 - select
        [HttpGet("gpg44")]
        public async Task<IActionResult> GPG44()
        {
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            QualityLevelViewModel qualityLevelViewModel = new();
            var qualityLevels = await editService.GetQualitylevels();
            qualityLevelViewModel.AvailableQualityOfAuthenticators = qualityLevels.Where(x => x.QualityType == QualityTypeEnum.Authentication).ToList();
            qualityLevelViewModel.SelectedQualityofAuthenticatorIds = serviceSummary?.QualityLevelViewModel?.SelectedQualityofAuthenticators?.Select(c => c.Id).ToList();

            qualityLevelViewModel.AvailableLevelOfProtections = qualityLevels.Where(x => x.QualityType == QualityTypeEnum.Protection).ToList();
            qualityLevelViewModel.SelectedLevelOfProtectionIds = serviceSummary?.QualityLevelViewModel?.SelectedLevelOfProtections?.Select(c => c.Id).ToList();

            qualityLevelViewModel.FromSummaryPage = serviceSummary.FromSummaryPage;
            ViewBag.serviceId = serviceSummary.ServiceId;

            return View(qualityLevelViewModel);
        }

        /// <summary>
        /// Save selected values to session
        /// </summary>
        /// <param name="qualityLevelViewModel"></param>
        /// <returns></returns>
        [HttpPost("gpg44")]
        public async Task<IActionResult> GPG44(QualityLevelViewModel qualityLevelViewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            List<QualityLevelDto> availableQualityLevels = await editService.GetQualitylevels();
            qualityLevelViewModel.AvailableQualityOfAuthenticators = availableQualityLevels.Where(x => x.QualityType == QualityTypeEnum.Authentication).ToList();
            qualityLevelViewModel.SelectedQualityofAuthenticatorIds = qualityLevelViewModel.SelectedQualityofAuthenticatorIds ?? [];
            if (qualityLevelViewModel.SelectedQualityofAuthenticatorIds.Count > 0)
                summaryViewModel.QualityLevelViewModel.SelectedQualityofAuthenticators = availableQualityLevels.Where(c => qualityLevelViewModel.SelectedQualityofAuthenticatorIds.Contains(c.Id)).ToList();

            qualityLevelViewModel.AvailableLevelOfProtections = availableQualityLevels.Where(x => x.QualityType == QualityTypeEnum.Protection).ToList();
            qualityLevelViewModel.SelectedLevelOfProtectionIds = qualityLevelViewModel.SelectedLevelOfProtectionIds ?? [];
            if (qualityLevelViewModel.SelectedLevelOfProtectionIds.Count > 0)
                summaryViewModel.QualityLevelViewModel.SelectedLevelOfProtections = availableQualityLevels.Where(c => qualityLevelViewModel.SelectedLevelOfProtectionIds.Contains(c.Id)).ToList();

            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

                return RedirectToAction("ServiceSummary");

            }
            else
            {
                return View("GPG44", qualityLevelViewModel);
            }
        }
        #endregion

        #region GPG45 - input

        [HttpGet("gpg45-input")]
        public async Task<IActionResult> GPG45Input(bool fromSummaryPage, int serviceId)
        {
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

            return View(summaryViewModel);
        }

        [HttpPost("gpg45-input")]
        public async Task<IActionResult> GPG45Input(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            if (ModelState["HasGPG45"].Errors.Count == 0)
            {
                summaryViewModel.HasGPG45 = serviceSummaryViewModel.HasGPG45;


                if (Convert.ToBoolean(summaryViewModel.HasGPG45))
                {
                    HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                    return RedirectToAction("GPG45");

                }
                else
                {
                    summaryViewModel.IdentityProfileViewModel.SelectedIdentityProfiles = [];
                    HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                    return RedirectToAction("ServiceSummary");
                }


            }
            else
            {
                return View("GPG45Input", serviceSummaryViewModel);
            }
        }
        #endregion

        #region GPG45 - select

        [HttpGet("gpg45")]
        public async Task<IActionResult> GPG45()
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            IdentityProfileViewModel identityProfileViewModel = new();
            identityProfileViewModel.SelectedIdentityProfileIds = summaryViewModel?.IdentityProfileViewModel?.SelectedIdentityProfiles?.Select(c => c.Id).ToList();
            identityProfileViewModel.AvailableIdentityProfiles = await editService.GetIdentityProfiles();

            identityProfileViewModel.FromSummaryPage = summaryViewModel.FromSummaryPage;
            ViewBag.serviceId = summaryViewModel.ServiceId;

            return View(identityProfileViewModel);
        }

        /// <summary>
        /// Save selected values to session
        /// </summary>
        /// <param name="identityProfileViewModel"></param>
        /// <returns></returns>
        [HttpPost("gpg45")]
        public async Task<IActionResult> GPG45(IdentityProfileViewModel identityProfileViewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            List<IdentityProfileDto> availableIdentityProfiles = await editService.GetIdentityProfiles();
            identityProfileViewModel.AvailableIdentityProfiles = availableIdentityProfiles;
            identityProfileViewModel.SelectedIdentityProfileIds = identityProfileViewModel.SelectedIdentityProfileIds ?? new List<int>();
            if (identityProfileViewModel.SelectedIdentityProfileIds.Count > 0)
                summaryViewModel.IdentityProfileViewModel.SelectedIdentityProfiles = availableIdentityProfiles.Where(c => identityProfileViewModel.SelectedIdentityProfileIds.Contains(c.Id)).ToList();
            summaryViewModel.IdentityProfileViewModel.FromSummaryPage = false;
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return RedirectToAction("ServiceSummary");

            }
            else
            {
                return View("GPG45", identityProfileViewModel);
            }
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
            var userEmails = await userService.GetUserEmailsExcludingLoggedIn(userEmail);

            ServiceChangesViewModel changesViewModel = new();
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();

            ServiceDto serviceDto = await editService.GetService(summaryViewModel.ServiceId);
            serviceDto.Provider.DSITUserEmails = string.Join(",", userEmails);

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
            var updatedRoleIds = updatedService.RoleViewModel.SelectedRoles.Select(m => m.Id).ToList(); ;

            var existingProtectionIds = existingService.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Protection)
                .Select(item => item.QualityLevelId);
            var updatedProtectionIds = updatedService.QualityLevelViewModel.SelectedLevelOfProtections
                .Select(item => item.Id);

            var existingAuthenticationIds = existingService.ServiceQualityLevelMapping
                .Where(item => item.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                .Select(item => item.QualityLevelId);
            var updatedAuthenticationIds = updatedService.QualityLevelViewModel.SelectedQualityofAuthenticators
                .Select(item => item.Id);

            var existingIdentityProfileIds = existingService.ServiceIdentityProfileMapping.Select(m => m.IdentityProfileId).ToList();
            var updatedIdentityProfileIds = updatedService.IdentityProfileViewModel.SelectedIdentityProfiles.Select(m => m.Id).ToList();

            var existingSupSchemeIds = existingService.ServiceSupSchemeMapping.Select(m => m.SupplementarySchemeId).ToList();
            var updatedSupSchemeIds = updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemes.Select(m => m.Id).ToList();

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
                    draft.ServiceRoleMappingDraft.Add(new ServiceRoleMappingDraftDto { Role = item});
                    
                }
            }
            if (!existingProtectionIds.OrderBy(id => id).SequenceEqual(updatedProtectionIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto {QualityLevel = item});
                }
            }

            if (!existingAuthenticationIds.OrderBy(id => id).SequenceEqual(updatedAuthenticationIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedQualityofAuthenticators)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevel = item });
                }
            }

            if (!existingIdentityProfileIds.OrderBy(id => id).SequenceEqual(updatedIdentityProfileIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.IdentityProfileViewModel.SelectedIdentityProfiles)
                {
                    draft.ServiceIdentityProfileMappingDraft.Add(new ServiceIdentityProfileMappingDraftDto { IdentityProfile = item}); 
                }
            }

            if (!existingSupSchemeIds.OrderBy(id => id).SequenceEqual(updatedSupSchemeIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevel = item});
                }
            }

            return (draft);
        }
        #endregion
    }
}
