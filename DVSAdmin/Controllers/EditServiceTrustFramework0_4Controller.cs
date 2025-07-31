using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSAdmin.Models.CabTransfer;
using DVSAdmin.Models.Edit;
using DVSAdmin.Validations;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{

    //Controller for TF version 0.4 gamma screens
    [Route("edit-service")]
    public class EditServiceTrustFramework0_4Controller : BaseController
    {
        private readonly IEditService editService;        
        private readonly IUserService userService;
      
        public EditServiceTrustFramework0_4Controller(IEditService editService, IUserService userService)
        {
            this.editService = editService;            
            this.userService = userService;
           
        }

        #region GPG45 input

        [HttpGet("service/gpg45-input")]
        public IActionResult ServiceGPG45Input(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
            return View(summaryViewModel);
        }

        [HttpPost("service/gpg45-input")]
        public  IActionResult SaveServiceGPG45Input(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            if (ModelState["HasGPG45"].Errors.Count == 0)
            {
                summaryViewModel.HasGPG45 = serviceSummaryViewModel.HasGPG45;

                if (Convert.ToBoolean(summaryViewModel.HasGPG45))
                {
                    return RedirectToAction("ServiceGPG45");

                }
                else
                {
                    summaryViewModel.IdentityProfileViewModel.SelectedIdentityProfiles = [];
                    HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                    return RedirectToAction("ServiceSummary", "EditService");
                }
            }
            else
            {
                return View("ServiceGPG45Input", serviceSummaryViewModel);
            }
        }
        #endregion
        #region select GPG45

        [HttpGet("service/gpg45")]
        public async Task<IActionResult> ServiceGPG45()
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
        [HttpPost("service/gpg45")]
        public async Task<IActionResult> SaveServiceGPG45(IdentityProfileViewModel identityProfileViewModel)
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
                return RedirectToAction("ServiceSummary", "EditService");

            }
            else
            {
                return View("ServiceGPG45", identityProfileViewModel);
            }
        }
        #endregion
        #region GPG44 - input

        [HttpGet("service/gpg44-input")]
        public IActionResult ServiceGPG44Input(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

            return View(summaryViewModel);
        }

        [HttpPost("service/gpg44-input")]
        public IActionResult SaveServiceGPG44Input(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            if (ModelState["HasGPG44"].Errors.Count == 0)
            {
                serviceSummary.HasGPG44 = serviceSummaryViewModel.HasGPG44;
                if (Convert.ToBoolean(serviceSummary.HasGPG44))
                {
                    return RedirectToAction("ServiceGPG44");
                }
                else
                {
                    // clear selections if the value is changed from yes to no
                    serviceSummary.QualityLevelViewModel.SelectedQualityofAuthenticators = new List<QualityLevelDto>();
                    serviceSummary.QualityLevelViewModel.SelectedLevelOfProtections = new List<QualityLevelDto>();
                    serviceSummary.SchemeQualityLevelMapping.ForEach(item => item.HasGPG44 = false);
                    serviceSummary.SchemeQualityLevelMapping.ForEach(item =>
                    {
                        if (item.QualityLevel != null)
                        {
                            item.QualityLevel.SelectedQualityofAuthenticators = [];
                            item.QualityLevel.SelectedLevelOfProtections = [];
                        }
                    });


                    HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                    return RedirectToAction("ServiceSummary", "EditService");

                }
            }
            else
            {
                return View("ServiceGPG44Input", serviceSummary);
            }
        }

        #endregion

        #region select GPG44
        [HttpGet("service/gpg44")]
        public async Task<IActionResult> ServiceGPG44(bool fromSummaryPage, bool fromDetailsPage)
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
        [HttpPost("service/gpg44")]
        public async Task<IActionResult> SaveServiceGPG44(QualityLevelViewModel qualityLevelViewModel, string action)
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
                return RedirectToAction("ServiceSummary", "EditService");

            }
            else
            {
                return View("GPG44", qualityLevelViewModel);
            }
        }
        #endregion

        #region Status of under pinning service - Is in register or not
        [HttpGet("status-of-underpinning-service")]
        public IActionResult StatusOfUnderpinningService(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();            
            return View(summaryViewModel);
        }

        [HttpPost("status-of-underpinning-service")]
        public  IActionResult StatusOfUnderpinningService(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            serviceSummary.FromSummaryPage = serviceSummaryViewModel.FromSummaryPage;           
            

            if (ModelState["IsUnderpinningServicePublished"].Errors.Count == 0)
            {
                serviceSummary.IsUnderpinningServicePublished = serviceSummaryViewModel.IsUnderpinningServicePublished;
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return RedirectToAction("SelectUnderpinningService");                
            }
            else
            {
                return View("StatusOfUnderpinningService", serviceSummaryViewModel);
            }
        }

        #endregion

        #region Search for under pinning / published based on status
        /// <summary>
        /// Search for services with status published or certificate review 
        /// passed based on selection in StatusOfUnderpinningService
        /// </summary>
        /// <param name="SearchText"></param>
        /// <param name="SearchAction"></param>
        /// <returns></returns>
        [HttpGet("select-underpinning-service")]
        public async Task<IActionResult> SelectUnderpinningService(string SearchText, string SearchAction)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            ViewBag.fromSummaryPage = summaryViewModel.FromSummaryPage;            

            var published = summaryViewModel.IsUnderpinningServicePublished == true;

            var services = new List<ServiceDto>();
            var manualServices = new List<ServiceDto>();
            if (SearchAction == "clearSearch")
            {
                ModelState.Clear();
                SearchText = string.Empty;
            }
            else if (SearchAction == "search" && string.IsNullOrEmpty(SearchText))
            {
                SearchText = "All";
            }

            if (SearchText != null)
            {
                if (published)
                {
                    var currentSelectedServiceId = summaryViewModel.SelectedUnderPinningServiceId;
                    // Fetch only services of type underpinning and status published
                    //IsUnderpinningServicePublished // true
                    //SelectedUnderPinningServiceId // not null
                    // SelectedManualUnderPinningServiceId //null
                    services = await editService.GetPublishedUnderpinningServices(SearchText, currentSelectedServiceId);
                }
                else
                {
                    var currentSelectedServiceId = summaryViewModel.SelectedManualUnderPinningServiceId;
                    //Fetch only manually saved underpinning services with certificate review passed status
                    // Fetch only services of type underpinning and status published
                    //IsUnderpinningServicePublished // false
                    //SelectedUnderPinningServiceId //  null
                    // SelectedManualUnderPinningServiceId //not null
                    manualServices = await editService.GetServicesWithManualUnderinningService(SearchText, currentSelectedServiceId);
                }

            }
            else
            {
                SearchText = string.Empty;

            }
            UnderpinningServiceViewModel underpinningServiceViewModel = new()
            {
                IsPublished = published,
                SearchText = SearchText,
                UnderpinningServices = services,
                ManualUnderpinningServices = manualServices                
            };
            return View(underpinningServiceViewModel);
        }

        [HttpGet("selected-underpinning-service")]

        public async Task<IActionResult> SelectedUnderpinningService(int serviceId, bool published, bool fromSummaryPage)
        {
            //under pinning service or whitelabel service details with manual service details
            var service = await editService.GetService(serviceId);
            ViewBag.published = published;
            ViewBag.fromSummaryPage = fromSummaryPage;           
            return View(service);
        }


        [HttpGet("confirm-underpinning-service")]
        public async Task<IActionResult> ConfirmUnderpinningService(int serviceId, bool published, bool fromSummaryPage)
        {           
            ViewBag.fromSummaryPage = fromSummaryPage;            
            var service = await editService.GetService(serviceId); //under pinning service or whitelabel service details with manual service details
            ServiceSummaryViewModel serviceSummaryInSession = GetServiceSummary();
            ServiceSummaryViewModel serviceSummary;
            if (published)
            {
                serviceSummary = new()
                {
                    SelectedUnderPinningServiceId = service.Id,
                    IsUnderpinningServicePublished = published,
                    UnderPinningProviderName = service.Provider.RegisteredName,
                    UnderPinningServiceName = service.ServiceName,
                    UnderPinningServiceExpiryDate = service.ConformityExpiryDate,
                    SelectCabViewModel = new SelectCabViewModel { SelectedCabId = service?.CabUser?.CabId, SelectedCabName = service?.CabUser?.Cab?.CabName },
                    ServiceKey = serviceSummaryInSession.ServiceKey


                };

            }
            else
            {
                serviceSummary = new()
                {
                    SelectedManualUnderPinningServiceId = service.ManualUnderPinningService.Id,
                    IsUnderpinningServicePublished = published,
                    UnderPinningProviderName = service.ManualUnderPinningService.ProviderName,
                    UnderPinningServiceName = service.ManualUnderPinningService.ServiceName,
                    UnderPinningServiceExpiryDate = service.ManualUnderPinningService.CertificateExpiryDate,
                    SelectCabViewModel = new SelectCabViewModel { SelectedCabId = service?.ManualUnderPinningService?.CabId, SelectedCabName = service?.ManualUnderPinningService?.Cab?.CabName },
                    ServiceKey = serviceSummaryInSession.ServiceKey
                };

            }

            return View(serviceSummary);
        }
        [HttpPost("confirm-underpinning-service")]
        public  IActionResult  SaveUnderpinningService(ServiceSummaryViewModel serviceSummaryViewModel)
        {
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();        
            serviceSummaryViewModel.FromSummaryPage = false;
            serviceSummary.SelectedManualUnderPinningServiceId = serviceSummaryViewModel.SelectedManualUnderPinningServiceId;
            serviceSummary.SelectedUnderPinningServiceId = serviceSummaryViewModel.SelectedUnderPinningServiceId;
            serviceSummary.UnderPinningServiceName = serviceSummaryViewModel.UnderPinningServiceName;
            serviceSummary.UnderPinningProviderName = serviceSummaryViewModel.UnderPinningProviderName;
            serviceSummary.UnderPinningServiceExpiryDate = serviceSummaryViewModel.UnderPinningServiceExpiryDate;
            serviceSummary.SelectCabViewModel = new SelectCabViewModel
            {
                SelectedCabId = serviceSummaryViewModel?.SelectCabViewModel?.SelectedCabId,
                SelectedCabName = serviceSummaryViewModel?.SelectCabViewModel?.SelectedCabName
            };
            serviceSummary.IsManualServiceLinkedToMultipleServices = true;

            if (ModelState["SelectedUnderPinningServiceId"].Errors.Count == 0 && ModelState["UnderPinningServiceName"].Errors.Count == 0)
            {
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return RedirectToAction("ServiceSummary", "EditService");
            }
            else
            {
                return View("ConfirmUnderpinningService", serviceSummaryViewModel);
            }
        }
        #endregion


        #region GPG45 - Scheme

        [HttpGet("scheme/gpg45")]
        public async Task<IActionResult> SchemeGPG45(bool fromSummaryPage, int schemeId)
        {

            ViewBag.fromSummaryPage = fromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();         
            var identityProfile = summaryViewModel?.SchemeIdentityProfileMapping?.Where(scheme => scheme.SchemeId == schemeId)?.FirstOrDefault()?.IdentityProfile;
            IdentityProfileViewModel identityProfileViewModel = new()
            {
                SchemeId = schemeId,
                SchemeName = summaryViewModel?.SupplementarySchemeViewModel?.SelectedSupplementarySchemes?.Where(scheme => scheme.Id == schemeId)
                .Select(scheme => scheme.SchemeName).FirstOrDefault() ?? string.Empty,
                SelectedIdentityProfileIds = identityProfile?.SelectedIdentityProfiles?.Select(c => c.Id)?.ToList() ?? [],
                AvailableIdentityProfiles = await editService.GetIdentityProfiles(),
                ServiceKey = summaryViewModel.ServiceKey
            };

            return View(identityProfileViewModel);


        }

        /// <summary>
        /// Save selected values to session
        /// </summary>
        /// <param name="identityProfileViewModel"></param>
        /// <returns></returns>
        [HttpPost("scheme/gpg45")]
        public async Task<IActionResult> SaveSchemeGPG45(IdentityProfileViewModel identityProfileViewModel)
        {
            bool fromSummaryPage = identityProfileViewModel.FromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            
            List<IdentityProfileDto> availableIdentityProfiles = await editService.GetIdentityProfiles();
            identityProfileViewModel.AvailableIdentityProfiles = availableIdentityProfiles;
            identityProfileViewModel.SelectedIdentityProfileIds = identityProfileViewModel.SelectedIdentityProfileIds ?? new List<int>();

            if (identityProfileViewModel.SelectedIdentityProfileIds.Count > 0)
            {
                var selectedServiceGpg45 = summaryViewModel.IdentityProfileViewModel.SelectedIdentityProfiles.Select(x => x.Id).ToList();
                if (identityProfileViewModel.SelectedIdentityProfileIds.Except(selectedServiceGpg45).Any())
                {
                    ModelState.AddModelError("SelectedIdentityProfileIds", Constants.NotGpg45SubsetError);
                }

                identityProfileViewModel.SelectedIdentityProfiles = availableIdentityProfiles.Where(c => identityProfileViewModel.SelectedIdentityProfileIds.Contains(c.Id)).ToList();
                SchemeIdentityProfileMappingViewModel schemeIdentityProfileMappingViewModel = new();
                schemeIdentityProfileMappingViewModel.SchemeId = identityProfileViewModel.SchemeId;
                schemeIdentityProfileMappingViewModel.IdentityProfile = identityProfileViewModel;
                schemeIdentityProfileMappingViewModel.IdentityProfile.SelectedIdentityProfiles = availableIdentityProfiles.Where(c => identityProfileViewModel.SelectedIdentityProfileIds.Contains(c.Id)).ToList();


                var existingMapping = summaryViewModel?.SchemeIdentityProfileMapping?.FirstOrDefault(x => x.SchemeId == schemeIdentityProfileMappingViewModel.SchemeId);
                if (existingMapping != null)
                {
                    int index = summaryViewModel.SchemeIdentityProfileMapping.IndexOf(existingMapping);
                    summaryViewModel.SchemeIdentityProfileMapping[index] = schemeIdentityProfileMappingViewModel;
                }
                else
                {
                    summaryViewModel?.SchemeIdentityProfileMapping?.Add(schemeIdentityProfileMappingViewModel);
                }
            }

            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);             
                int nextMissingSchemeIdForGpg44 = NextMissingSchemId("GPG44");              
                 if (nextMissingSchemeIdForGpg44 > 0)
                    return RedirectToAction("SchemeGPG44Input", "EditServiceTrustFramework0_4", new { fromSummaryPage = fromSummaryPage, schemeId = nextMissingSchemeIdForGpg44 });
                else
                    return RedirectToAction("ServiceSummary", "EditService");
              
            }
            else
            {
                identityProfileViewModel.ServiceKey = summaryViewModel.ServiceKey;
                return View("SchemeGPG45", identityProfileViewModel);
            }
        }

        
        #endregion

        #region GPG44 - input - scheme

        [HttpGet("scheme/gpg44-input")]
        public IActionResult SchemeGPG44Input(bool fromSummaryPage, int schemeId)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();

            SchemeQualityLevelMappingViewModel schemeQualityLevelMappingViewModel = summaryViewModel?.SchemeQualityLevelMapping?.Where(scheme => scheme.SchemeId == schemeId).
            FirstOrDefault() ?? new();            
            schemeQualityLevelMappingViewModel.SchemeId = schemeId;
            schemeQualityLevelMappingViewModel.SchemeName = summaryViewModel?.SupplementarySchemeViewModel?.SelectedSupplementarySchemes?.Where(scheme => scheme.Id == schemeId)
            .Select(scheme => scheme.SchemeName).FirstOrDefault() ?? string.Empty;
            schemeQualityLevelMappingViewModel.ServiceKey = summaryViewModel.ServiceKey;
            return View(schemeQualityLevelMappingViewModel);
        }


        [HttpPost("scheme/gpg44-input")]
        public async Task<IActionResult> SaveSchemeGPG44Input(SchemeQualityLevelMappingViewModel viewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            bool fromSummaryPage = viewModel.FromSummaryPage;

            if (summaryViewModel.HasGPG44 == false && viewModel.HasGPG44 == true)
            {
                ModelState.AddModelError("HasGPG44", Constants.ServiceGpg44SelectedNo);
            }
            if (ModelState["HasGPG44"].Errors.Count == 0)
            {
                SchemeQualityLevelMappingViewModel schemeQualityLevelMappingViewModel = new();

                var existingMapping = summaryViewModel?.SchemeQualityLevelMapping?.FirstOrDefault(x => x.SchemeId == viewModel.SchemeId);              
                if (existingMapping != null)
                {
                    int index = summaryViewModel.SchemeQualityLevelMapping.IndexOf(existingMapping);
                    schemeQualityLevelMappingViewModel = viewModel;
                    summaryViewModel.SchemeQualityLevelMapping[index].HasGPG44 = viewModel.HasGPG44;
                    HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                    if (viewModel.HasGPG44 == true)
                    {
                        return RedirectToAction("SchemeGPG44", new { fromSummaryPage = fromSummaryPage, schemeId = viewModel.SchemeId });
                    }
                    else
                    {
                        summaryViewModel.SchemeQualityLevelMapping[index].QualityLevel = new();
                        HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                        return RedirectToAction("ServiceSummary", "EditService");
                    }
                }
                else
                {
                    schemeQualityLevelMappingViewModel.HasGPG44 = viewModel.HasGPG44;
                    schemeQualityLevelMappingViewModel.SchemeId = viewModel.SchemeId;                    
                    summaryViewModel?.SchemeQualityLevelMapping?.Add(schemeQualityLevelMappingViewModel);
                    HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                    if (viewModel.HasGPG44 == true)
                    {
                        return RedirectToAction("SchemeGPG44", new { fromSummaryPage = fromSummaryPage, schemeId = viewModel.SchemeId });
                    }
                    else
                    {                       
                       
                        return RedirectToAction("ServiceSummary", "EditService");
                    }

                }
            }
            else
            {
                viewModel.ServiceKey = summaryViewModel.ServiceKey;
                return View("SchemeGPG44Input", viewModel);
            }
        }

        #endregion
        #region select GPG44 - Scheme
        [HttpGet("scheme/gpg44")]
        public async Task<IActionResult> SchemeGPG44(bool fromSummaryPage, int schemeId)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            QualityLevelViewModel qualityLevelViewModel = new()
            {
                SchemeId = schemeId,
                SchemeName = summaryViewModel?.SupplementarySchemeViewModel?.SelectedSupplementarySchemes?.Where(scheme => scheme.Id == schemeId)
                .Select(scheme => scheme.SchemeName).FirstOrDefault() ?? string.Empty,
                ServiceKey = summaryViewModel.ServiceKey
            };
            var qualityLevel = summaryViewModel?.SchemeQualityLevelMapping?.Where(scheme => scheme.SchemeId == schemeId)?.FirstOrDefault()?.QualityLevel;


            var qualityLevels = await editService.GetQualitylevels();
            qualityLevelViewModel.AvailableQualityOfAuthenticators = qualityLevels.Where(x => x.QualityType == QualityTypeEnum.Authentication).ToList();
            qualityLevelViewModel.SelectedQualityofAuthenticatorIds = qualityLevel?.SelectedQualityofAuthenticators?.Select(c => c.Id)?.ToList() ?? [];

            qualityLevelViewModel.AvailableLevelOfProtections = qualityLevels.Where(x => x.QualityType == QualityTypeEnum.Protection).ToList();
            qualityLevelViewModel.SelectedLevelOfProtectionIds = qualityLevel?.SelectedLevelOfProtections?.Select(c => c.Id)?.ToList() ?? [];


            return View(qualityLevelViewModel);
        }

        /// <summary>
        /// Save selected values to session
        /// </summary>
        /// <param name="qualityLevelViewModel"></param>
        /// <returns></returns>
        [HttpPost("scheme/gpg44")]
        public async Task<IActionResult> SaveSchemeGPG44(QualityLevelViewModel qualityLevelViewModel)
        {

            bool fromSummaryPage = qualityLevelViewModel.FromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            
            List<QualityLevelDto> availableQualityLevels = await editService.GetQualitylevels();
            qualityLevelViewModel.AvailableQualityOfAuthenticators = availableQualityLevels.Where(x => x.QualityType == QualityTypeEnum.Authentication).ToList();
            qualityLevelViewModel.SelectedQualityofAuthenticatorIds = qualityLevelViewModel.SelectedQualityofAuthenticatorIds ?? [];
            qualityLevelViewModel.AvailableLevelOfProtections = availableQualityLevels.Where(x => x.QualityType == QualityTypeEnum.Protection).ToList();
            qualityLevelViewModel.SelectedLevelOfProtectionIds = qualityLevelViewModel.SelectedLevelOfProtectionIds ?? [];
            if (qualityLevelViewModel.SelectedQualityofAuthenticatorIds.Count > 0 && qualityLevelViewModel.SelectedLevelOfProtectionIds.Count > 0)
            {
                var selectedServiceGpg44Authenticator = summaryViewModel.QualityLevelViewModel.SelectedQualityofAuthenticators.Select(x => x.Id).ToList();

                var selectedServiceGpg44Protection = summaryViewModel.QualityLevelViewModel.SelectedLevelOfProtections.Select(x => x.Id).ToList();
                if (qualityLevelViewModel.SelectedQualityofAuthenticatorIds.Except(selectedServiceGpg44Authenticator).Any())
                {
                    ModelState.AddModelError("SelectedQualityofAuthenticatorIds", Constants.NotGpg44SubsetError);
                }
                if (qualityLevelViewModel.SelectedLevelOfProtectionIds.Except(selectedServiceGpg44Protection).Any())
                {
                    ModelState.AddModelError("SelectedLevelOfProtectionIds", Constants.NotGpg44SubsetError);
                }

                qualityLevelViewModel.SelectedQualityofAuthenticators = availableQualityLevels.Where(c => qualityLevelViewModel.SelectedQualityofAuthenticatorIds.Contains(c.Id)).ToList();
                qualityLevelViewModel.SelectedLevelOfProtections = availableQualityLevels.Where(c => qualityLevelViewModel.SelectedLevelOfProtectionIds.Contains(c.Id)).ToList();
                var mapping = summaryViewModel.SchemeQualityLevelMapping?.Where(x => x.SchemeId == qualityLevelViewModel.SchemeId).FirstOrDefault() ?? new SchemeQualityLevelMappingViewModel();
                mapping.QualityLevel = qualityLevelViewModel;

            }
            summaryViewModel.QualityLevelViewModel.FromSummaryPage = false;
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

                int nextMissingSchemeIdForGpg45 = NextMissingSchemId("GPG45");              
                if (nextMissingSchemeIdForGpg45 > 0)
                    return RedirectToAction("SchemeGPG45", "EditServiceTrustFramework0_4", new { fromSummaryPage = fromSummaryPage, schemeId = nextMissingSchemeIdForGpg45 });               
                else
                    return RedirectToAction("ServiceSummary", "EditService");

            }
            else
            {
                qualityLevelViewModel.ServiceKey = summaryViewModel.ServiceKey;
                return View("SchemeGPG44", qualityLevelViewModel);
            }
        }
        #endregion


        #region Service Name
        [HttpGet("underpinning-service-name")]
        public IActionResult UnderPinningServiceName(bool fromSummaryPage, bool fromUnderPinningServiceSummaryPage, bool manualEntry)
        {

            ViewBag.fromSummaryPage = fromSummaryPage;
            ViewBag.fromUnderPinningServiceSummaryPage = fromUnderPinningServiceSummaryPage;
            ViewBag.manualEntryFirstTimeLoad = manualEntry;
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();
            if (manualEntry)
            {
                ViewModelHelper.ClearUnderPinningServiceFieldsBeforeManualEntry(serviceSummaryViewModel);
                serviceSummaryViewModel.ManualEntryFirstTimeLoad = manualEntry;
                HttpContext?.Session.Set("ServiceSummary", serviceSummaryViewModel);
            }
          
            return View(serviceSummaryViewModel);
        }

        [HttpPost("underpinning-service-name")]
        public IActionResult SaveUnderPinningServiceName(ServiceSummaryViewModel serviceSummaryViewModel)
        {      
                    
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            if (ModelState["UnderPinningServiceName"].Errors.Count == 0)
            {
                serviceSummary.UnderPinningServiceName = serviceSummaryViewModel.UnderPinningServiceName;               
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return  serviceSummaryViewModel.FromUnderPinningServiceSummaryPage ? RedirectToAction("UnderpinningServiceDetailsSummary") 
                : serviceSummary.ManualEntryFirstTimeLoad ? RedirectToAction("UnderPinningProviderName") 
                : RedirectToAction("ServiceSummary", "EditService");
            }
            else
            {
              return  View("UnderPinningServiceName", serviceSummaryViewModel);
            }
        }
        #endregion

        #region Underpinning provider name

        [HttpGet("underpinning-provider-name")]
        public IActionResult UnderPinningProviderName(bool fromSummaryPage, bool fromUnderPinningServiceSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ViewBag.fromUnderPinningServiceSummaryPage = fromUnderPinningServiceSummaryPage;        
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();            
            return View(serviceSummaryViewModel);
        }

        [HttpPost("underpinning-provider-name")]
        public IActionResult SaveUnderPinningProviderName(ServiceSummaryViewModel serviceSummaryViewModel)
        {         
            
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            if (ModelState["UnderPinningProviderName"].Errors.Count == 0)
            {
                serviceSummary.UnderPinningProviderName = serviceSummaryViewModel.UnderPinningProviderName;
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return serviceSummaryViewModel.FromUnderPinningServiceSummaryPage ? RedirectToAction("UnderpinningServiceDetailsSummary")
                : serviceSummary.ManualEntryFirstTimeLoad ? RedirectToAction("SelectCabOfUnderpinningService")
                : RedirectToAction("ServiceSummary", "EditService");
            }
            else
            {
                return View("UnderPinningProviderName", serviceSummaryViewModel);
            }
        }
        #endregion

        #region Select-cab

        [HttpGet("select-cab")]
        public async Task<IActionResult> SelectCabOfUnderpinningService(bool fromSummaryPage, bool fromUnderPinningServiceSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;            
            ViewBag.fromUnderPinningServiceSummaryPage = fromUnderPinningServiceSummaryPage;            
            var allCabs = await editService.GetAllCabs();
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();
            var selectCabViewModel = serviceSummaryViewModel?.SelectCabViewModel ?? new SelectCabViewModel();
            selectCabViewModel.Cabs = allCabs;
            selectCabViewModel.ServiceKey = serviceSummaryViewModel.ServiceKey;
            return View(selectCabViewModel);
        }

        [HttpPost("select-cab")]
        public async Task<IActionResult> SaveSelectedCab(SelectCabViewModel cabsViewModel)
        {
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();
            if (cabsViewModel.Cabs == null)
                cabsViewModel.Cabs = await editService.GetAllCabs();
            if (ModelState["SelectedCabId"].Errors.Count == 0)
            {
                string cabName = cabsViewModel.Cabs.Where(x => x.Id == cabsViewModel.SelectedCabId).Select(x => x.CabName).First();
                serviceSummary.SelectCabViewModel = new SelectCabViewModel
                {
                    SelectedCabId = cabsViewModel.SelectedCabId,
                    SelectedCabName = cabName
                };
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return cabsViewModel.FromUnderPinningServiceSummaryPage ? RedirectToAction("UnderpinningServiceDetailsSummary")
               : serviceSummary.ManualEntryFirstTimeLoad ? 
                 RedirectToAction("UnderPinningServiceExpiryDate")
               : RedirectToAction("ServiceSummary", "EditService");
            }
            cabsViewModel.ServiceKey = serviceSummary.ServiceKey;
            return View("SelectCabOfUnderpinningService", cabsViewModel);
        }
        #endregion


        #region Expiry date

        [HttpGet("under-pinning-service-expiry-date")]
        public IActionResult UnderPinningServiceExpiryDate(bool fromSummaryPage,  bool fromUnderPinningServiceSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ViewBag.fromUnderPinningServiceSummaryPage = fromUnderPinningServiceSummaryPage;
          
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            DateViewModel dateViewModel = new()
            {
                PropertyName = "UnderPinningServiceExpiryDate"
            };

            if (summaryViewModel.UnderPinningServiceExpiryDate != null)
            {
                dateViewModel = ViewModelHelper.GetDayMonthYear(summaryViewModel.UnderPinningServiceExpiryDate);
            }
           
            return View(dateViewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateViewModel"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        [HttpPost("under-pinning-service-expiry-date")]
        public IActionResult SaveUnderPinningServiceExpiryDate(DateViewModel dateViewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary(); 
            dateViewModel.PropertyName = "UnderPinningServiceExpiryDate";
            DateTime? underPinningServiceExpiryDate = ValidationHelper.ValidateCustomExpiryDate(dateViewModel, Convert.ToDateTime(summaryViewModel.ConformityIssueDate), ModelState);
            
            if (ModelState.IsValid)
            {
                summaryViewModel.UnderPinningServiceExpiryDate = underPinningServiceExpiryDate;
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return dateViewModel.FromSummaryPage ? RedirectToAction("ServiceSummary", "EditService")
               : RedirectToAction("UnderpinningServiceDetailsSummary");
            }
            else
            {
                return View("UnderPinningServiceExpiryDate", dateViewModel);
            }
        }
        #endregion


        [HttpGet("underpinning-service-details-summary")]
        public IActionResult UnderpinningServiceDetailsSummary()
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.ManualEntryFirstTimeLoad = false;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
           
            return View(summaryViewModel);
        }

        
    }
}
