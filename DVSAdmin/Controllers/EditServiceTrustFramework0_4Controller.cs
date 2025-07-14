using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
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
    public class EditServiceTrustFramework0_4Controller : BaseController
    {
        private readonly IEditService editService;        
        private readonly IUserService userService;
      
        public EditServiceTrustFramework0_4Controller(IEditService editService, IUserService userService)
        {
            this.editService = editService;            
            this.userService = userService;
           
        }
        #region GPG45

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
                AvailableIdentityProfiles = await editService.GetIdentityProfiles()
            };

            return View(identityProfileViewModel);


        }

        /// <summary>
        /// Save selected values to session
        /// </summary>
        /// <param name="identityProfileViewModel"></param>
        /// <returns></returns>
        [HttpPost("scheme/gpg45")]
        public async Task<IActionResult> SaveSchemeGPG45(IdentityProfileViewModel identityProfileViewModel, string action)
        {
            bool fromSummaryPage = identityProfileViewModel.FromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            
            List<IdentityProfileDto> availableIdentityProfiles = await editService.GetIdentityProfiles();
            identityProfileViewModel.AvailableIdentityProfiles = availableIdentityProfiles;
            identityProfileViewModel.SelectedIdentityProfileIds = identityProfileViewModel.SelectedIdentityProfileIds ?? new List<int>();

            if (identityProfileViewModel.SelectedIdentityProfileIds.Count > 0)
            {
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
                return RedirectToMissingMappings(fromSummaryPage);
            }
            else
            {
                return View("SchemeGPG45", identityProfileViewModel);
            }
        }

        
        #endregion

        #region GPG44 - input

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

            return View(schemeQualityLevelMappingViewModel);
        }


        [HttpPost("scheme/gpg44-input")]
        public async Task<IActionResult> SaveSchemeGPG44Input(SchemeQualityLevelMappingViewModel viewModel)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            bool fromSummaryPage = viewModel.FromSummaryPage;                       
            viewModel.FromDetailsPage = false;
            viewModel.FromSummaryPage = false;
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
                return View("SchemeGPG44Input", viewModel);
            }
        }

        #endregion
        #region select GPG44
        [HttpGet("scheme/gpg44")]
        public async Task<IActionResult> SchemeGPG44(bool fromSummaryPage, int schemeId)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;            
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            QualityLevelViewModel qualityLevelViewModel = new()
            {
                SchemeId = schemeId,
                SchemeName = summaryViewModel?.SupplementarySchemeViewModel?.SelectedSupplementarySchemes?.Where(scheme => scheme.Id == schemeId)
                .Select(scheme => scheme.SchemeName).FirstOrDefault() ?? string.Empty
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
                qualityLevelViewModel.SelectedQualityofAuthenticators = availableQualityLevels.Where(c => qualityLevelViewModel.SelectedQualityofAuthenticatorIds.Contains(c.Id)).ToList();
                qualityLevelViewModel.SelectedLevelOfProtections = availableQualityLevels.Where(c => qualityLevelViewModel.SelectedLevelOfProtectionIds.Contains(c.Id)).ToList();
                var mapping = summaryViewModel.SchemeQualityLevelMapping?.Where(x => x.SchemeId == qualityLevelViewModel.SchemeId).FirstOrDefault() ?? new SchemeQualityLevelMappingViewModel();
                mapping.QualityLevel = qualityLevelViewModel;

            }
            summaryViewModel.QualityLevelViewModel.FromSummaryPage = false;
            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);
                return RedirectToMissingMappings(fromSummaryPage);

            }
            else
            {
                return View("SchemeGPG44", qualityLevelViewModel);
            }
        }
        #endregion


        #region Service Name
        [HttpGet("underpinning-service-name")]
        public IActionResult UnderPinningServiceName(bool fromSummaryPage)
        {

            ViewBag.fromSummaryPage = fromSummaryPage;     
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();
     
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
                return RedirectToAction("ServiceSummary", "EditService");
            }
            else
            {
                return View("UnderPinningServiceName", serviceSummaryViewModel);
            }
        }
        #endregion

        #region underpinning provider name

        [HttpGet("underpinning-provider-name")]
        public IActionResult UnderPinningProviderName(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;              
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();            
            return View(serviceSummaryViewModel);
        }

        [HttpPost("underpinning-provider-name")]
        public IActionResult SaveUnderPinningProviderName(ServiceSummaryViewModel serviceSummaryViewModel, string action)
        {         
            
            ServiceSummaryViewModel serviceSummary = GetServiceSummary();            
            
            if (ModelState["UnderPinningProviderName"].Errors.Count == 0)
            {
                serviceSummary.UnderPinningProviderName = serviceSummaryViewModel.UnderPinningProviderName;
                HttpContext?.Session.Set("ServiceSummary", serviceSummary);
                return RedirectToAction("ServiceSummary", "EditService");
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
                return RedirectToAction("ServiceSummary", "EditService");
            }

            return View("SelectCabOfUnderpinningService", cabsViewModel);
        }
        #endregion


        #region Expiry date

        [HttpGet("under-pinning-service-expiry-date")]
        public IActionResult UnderPinningServiceExpiryDate(bool fromSummaryPage,  bool fromUnderPinningServiceSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;           
            
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
                return RedirectToAction("ServiceSummary", "EditService");
            }
            else
            {
                return View("UnderPinningServiceExpiryDate", dateViewModel);
            }
        }
        #endregion
    }
}
