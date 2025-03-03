using CsvHelper.Configuration.Attributes;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Models;
using DVSAdmin.Models.Edit;
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
            ViewBag.serviceKey = serviceSummary.ServiceKey;

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
            summaryViewModel.HasGPG44 = true;
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
            ViewBag.serviceKey = summaryViewModel.ServiceKey;

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
            summaryViewModel.HasGPG45 = true;
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

        #region Supplemetary schemes - input

        [HttpGet("supplementary-schemes-input")]
        public async Task<IActionResult> SupplementarySchemesInput(bool fromSummaryPage, int serviceId)
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

        [HttpPost("supplementary-schemes-input")]
        public async Task<IActionResult> SupplementarySchemesInput(ServiceSummaryViewModel viewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            if (ModelState["HasSupplementarySchemes"].Errors.Count == 0)
            {
                summaryViewModel.HasSupplementarySchemes = viewModel.HasSupplementarySchemes;


                if (Convert.ToBoolean(summaryViewModel.HasSupplementarySchemes))
                {
                    return RedirectToAction("SupplementarySchemes");
                }
                else
                {
                    summaryViewModel.SupplementarySchemeViewModel.SelectedSupplementarySchemes = [];
                    HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                    return RedirectToAction("ServiceSummary");
                }
            }
            else
            {
                return View("SupplementarySchemesInput", summaryViewModel);
            }
        }
        #endregion

        #region Supplementary schemes - select

        [HttpGet("supplementary-schemes")]
        public async Task<IActionResult> SupplementarySchemes()
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            SupplementarySchemeViewModel supplementarySchemeViewModel = new()
            {
                SelectedSupplementarySchemeIds = summaryViewModel?.SupplementarySchemeViewModel?.SelectedSupplementarySchemes?.Select(c => c.Id).ToList(),
                AvailableSchemes = await editService.GetSupplementarySchemes()
            };

            supplementarySchemeViewModel.FromSummaryPage = summaryViewModel.FromSummaryPage;
            ViewBag.serviceId = summaryViewModel.ServiceId;
            ViewBag.serviceKey = summaryViewModel.ServiceKey;
            return View(supplementarySchemeViewModel);
        }

        [HttpPost("supplementary-schemes")]
        public async Task<IActionResult> SupplementarySchemes(SupplementarySchemeViewModel supplementarySchemeViewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.HasSupplementarySchemes = true;
            List<SupplementarySchemeDto> availableSupplementarySchemes = await editService.GetSupplementarySchemes();
            supplementarySchemeViewModel.AvailableSchemes = availableSupplementarySchemes;
            supplementarySchemeViewModel.SelectedSupplementarySchemeIds = supplementarySchemeViewModel.SelectedSupplementarySchemeIds ?? new List<int>();
            if (supplementarySchemeViewModel.SelectedSupplementarySchemeIds.Count > 0)
                summaryViewModel.SupplementarySchemeViewModel.SelectedSupplementarySchemes = availableSupplementarySchemes.Where(c => supplementarySchemeViewModel.SelectedSupplementarySchemeIds.Contains(c.Id)).ToList();
            summaryViewModel.SupplementarySchemeViewModel.FromSummaryPage = false;

            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return RedirectToAction("ServiceSummary");

            }
            else
            {
                return View("SupplementarySchemes", supplementarySchemeViewModel);
            }
        }
        #endregion

        #region Conformity Issue date
        [HttpGet("issue-date")]
        public async Task<IActionResult> ConformityIssueDate(bool fromSummaryPage, int serviceId)
        {
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            DateViewModel dateViewModel = new DateViewModel();
            dateViewModel.PropertyName = "ConformityIssueDate";
            if (summaryViewModel.ConformityIssueDate != null)
            {
                dateViewModel = GetDayMonthYear(summaryViewModel.ConformityIssueDate);
            }
            dateViewModel.FromSummaryPage = fromSummaryPage;
            ViewBag.serviceId = serviceId;
            return View(dateViewModel);
        }



        /// <summary>
        /// Updates confirmity issue date variable in session 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost("issue-date")]
        public async Task<IActionResult> ConformityIssueDate(DateViewModel dateViewModel)
        {
            dateViewModel.PropertyName = "ConformityIssueDate";
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            DateTime? conformityIssueDate = ValidateIssueDate(dateViewModel, summaryViewModel.ConformityExpiryDate, dateViewModel.FromSummaryPage);
            if (ModelState.IsValid)
            {
                summaryViewModel.ConformityIssueDate = conformityIssueDate;
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return RedirectToAction("ServiceSummary");                
            }
            else
            {
                return View("ConformityIssueDate", dateViewModel);
            }
        }
        #endregion

        #region Conformity Expiry date
        [HttpGet("expiry-date")]
        public async Task<IActionResult> ConformityExpiryDate(bool fromSummaryPage, int serviceId)
        {
            if (!fromSummaryPage)
            {
                ServiceDto serviceDto = await editService.GetService(serviceId);
                SetServiceDataToSession(serviceDto);
            }
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            DateViewModel dateViewModel = new DateViewModel();
            dateViewModel.PropertyName = "ConformityExpiryDate";
            if (summaryViewModel.ConformityExpiryDate != null)
            {
                dateViewModel = GetDayMonthYear(summaryViewModel.ConformityExpiryDate);
            }
            dateViewModel.FromSummaryPage = fromSummaryPage;
            ViewBag.serviceId = serviceId;
            return View(dateViewModel);
        }


        /// <summary>
        /// Updates confirmity issue expiry date variable in session 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost("expiry-date")]
        public async Task<IActionResult> ConformityExpiryDate(DateViewModel dateViewModel, string action)
        {
            dateViewModel.PropertyName = "ConformityExpiryDate";
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();

            DateTime? conformityExpiryDate = ValidateExpiryDate(dateViewModel, Convert.ToDateTime(summaryViewModel.ConformityIssueDate));

            if (ModelState.IsValid)
            {
                summaryViewModel.ConformityExpiryDate = conformityExpiryDate;
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return RedirectToAction("ServiceSummary");
            }
            else
            {
                return View("ConformityExpiryDate", dateViewModel);
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
            changesViewModel.DSITUserEmails = string.Join(",", userEmails);

            ServiceDraftDto serviceDraftDto = CreateDraft(serviceDto, summaryViewModel);

            HttpContext?.Session.Set("ChangedServiceData", serviceDraftDto);

            changesViewModel.CurrentService = serviceDto;
            changesViewModel.ChangedService = serviceDraftDto;

            return View(changesViewModel);
        }



        [HttpPost("summary-of-changes")]
        public async Task<IActionResult> SaveServiceDraft(ServiceChangesViewModel serviceChangesViewModel)
        {
            List<string> dsitUserEmails = serviceChangesViewModel.DSITUserEmails.Split(',').ToList();
            ServiceDraftDto serviceDraft = HttpContext.Session.Get<ServiceDraftDto>("ChangedServiceData");
            if (serviceDraft != null)
            {

                GenericResponse genericResponse = await editService.SaveServiceDraft(serviceDraft, userEmail, dsitUserEmails);
                if (genericResponse.Success)
                {
                    return RedirectToAction("InformationSubmitted", new { serviceId = serviceDraft.serviceId });
                }
                else
                {
                    return RedirectToAction("HandleException", "Error");
                }
            }
            else
            {
                return RedirectToAction("HandleException", "Error");
            }
          

        }


        /// <summary>
        ///Final page if save success
        /// </summary>       
        /// <returns></returns>
        [HttpGet("service-submitted")]
        public async Task<IActionResult> InformationSubmitted(int serviceId)
        {
            ServiceDto serviceDto = await editService.GetService(serviceId);
            HttpContext?.Session.Remove("ChangedServiceData");
            HttpContext?.Session.Remove("ServiceSummary");
            return View(serviceDto);

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
                ProviderProfileId = existingService.ProviderProfileId
            };

            

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
                    draft.ServiceRoleMappingDraft.Add(new ServiceRoleMappingDraftDto { RoleId = item.Id , Role = item});
                    
                }
            }
            if (!existingProtectionIds.OrderBy(id => id).SequenceEqual(updatedProtectionIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedLevelOfProtections)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id, QualityLevel = item});
                }
            }

            if (!existingAuthenticationIds.OrderBy(id => id).SequenceEqual(updatedAuthenticationIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.QualityLevelViewModel.SelectedQualityofAuthenticators)
                {
                    draft.ServiceQualityLevelMappingDraft.Add(new ServiceQualityLevelMappingDraftDto { QualityLevelId = item.Id, QualityLevel = item });
                }
            }

            if (!existingIdentityProfileIds.OrderBy(id => id).SequenceEqual(updatedIdentityProfileIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.IdentityProfileViewModel.SelectedIdentityProfiles)
                {
                    draft.ServiceIdentityProfileMappingDraft.Add(new ServiceIdentityProfileMappingDraftDto {IdentityProfileId = item.Id,  IdentityProfile = item}); 
                }
            }

            if (!existingSupSchemeIds.OrderBy(id => id).SequenceEqual(updatedSupSchemeIds.OrderBy(id => id)))
            {
                foreach (var item in updatedService.SupplementarySchemeViewModel.SelectedSupplementarySchemes)
                {
                    draft.ServiceSupSchemeMappingDraft.Add(new ServiceSupSchemeMappingDraftDto {  SupplementarySchemeId = item.Id, SupplementaryScheme = item});
                }
            }

            return (draft);
        }

        private DateViewModel GetDayMonthYear(DateTime? dateTime)
        {
            DateViewModel dateViewModel = new DateViewModel();
            DateTime conformityIssueDate = Convert.ToDateTime(dateTime);
            dateViewModel.Day = conformityIssueDate.Day;
            dateViewModel.Month = conformityIssueDate.Month;
            dateViewModel.Year = conformityIssueDate.Year;
            return dateViewModel;
        }

        private DateTime? ValidateIssueDate(DateViewModel dateViewModel, DateTime? expiryDate, bool fromSummaryPage)
        {
            DateTime? date = null;
            DateTime minDate = new DateTime(1900, 1, 1);
            DateTime minIssueDate;


            try
            {
                if (dateViewModel.Day == null || dateViewModel.Month == null || dateViewModel.Year == null)
                {
                    if (dateViewModel.Day == null)
                    {
                        ModelState.AddModelError("Day", Constants.ConformityIssueDayError);
                    }
                    if (dateViewModel.Month == null)
                    {
                        ModelState.AddModelError("Month", Constants.ConformityIssueMonthError);
                    }
                    if (dateViewModel.Year == null)
                    {
                        ModelState.AddModelError("Year", Constants.ConformityIssueYearError);
                    }
                }
                else
                {
                    date = new DateTime(Convert.ToInt32(dateViewModel.Year), Convert.ToInt32(dateViewModel.Month), Convert.ToInt32(dateViewModel.Day));
                    if (date > DateTime.Today)
                    {
                        ModelState.AddModelError("ValidDate", Constants.ConformityIssuePastDateError);
                    }
                    if (date < minDate)
                    {
                        ModelState.AddModelError("ValidDate", Constants.ConformityIssueDateInvalidError);
                    }

                    if (expiryDate.HasValue && fromSummaryPage)
                    {
                        minIssueDate = expiryDate.Value.AddYears(-2).AddDays(-60);
                        if (date < minIssueDate)
                        {
                            ModelState.AddModelError("ValidDate", Constants.ConformityMaxExpiryDateError);
                        }
                    }

                }

            }
            catch (Exception)
            {
                ModelState.AddModelError("ValidDate", Constants.ConformityIssueDateInvalidError);

            }
            return date;
        }

        private DateTime? ValidateExpiryDate(DateViewModel dateViewModel, DateTime issueDate)
        {
            DateTime? date = null;

            try
            {
                if (dateViewModel.Day == null || dateViewModel.Month == null || dateViewModel.Year == null)
                {
                    if (dateViewModel.Day == null)
                    {
                        ModelState.AddModelError("Day", Constants.ConformityExpiryDayError);
                    }
                    if (dateViewModel.Month == null)
                    {
                        ModelState.AddModelError("Month", Constants.ConformityExpiryMonthError);
                    }
                    if (dateViewModel.Year == null)
                    {
                        ModelState.AddModelError("Year", Constants.ConformityExpiryYearError);
                    }
                }
                else
                {
                    date = new DateTime(Convert.ToInt32(dateViewModel.Year), Convert.ToInt32(dateViewModel.Month), Convert.ToInt32(dateViewModel.Day));
                    var maxExpiryDate = issueDate.AddYears(2).AddDays(60);
                    if (date <= DateTime.Today)
                    {
                        ModelState.AddModelError("ValidDate", Constants.ConformityExpiryPastDateError);
                    }
                    else if (date <= issueDate)
                    {
                        ModelState.AddModelError("ValidDate", Constants.ConformityIssueDateExpiryDateError);
                    }
                    else if (date > maxExpiryDate)
                    {
                        ModelState.AddModelError("ValidDate", Constants.ConformityMaxExpiryDateError);
                    }
                }

            }
            catch (Exception)
            {
                ModelState.AddModelError("ValidDate", Constants.ConformityExpiryDateInvalidError);

            }
            return date;
        }

        #endregion
    }
}
