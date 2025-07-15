using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSAdmin.Models.Edit;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DVSAdmin.Controllers
{
    [Route("edit-service")]   
    public class EditServiceController : BaseController
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
        public IActionResult ServiceName(bool fromSummaryPage)
        {          
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
        public IActionResult CompanyAddress(bool fromSummaryPage)
        {
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
        public async Task<IActionResult> ProviderRoles(bool fromSummaryPage)
        {
            ServiceSummaryViewModel serviceSummaryViewModel = GetServiceSummary();
            RoleViewModel roleViewModel = new RoleViewModel();
            roleViewModel.SelectedRoleIds = serviceSummaryViewModel?.RoleViewModel?.SelectedRoles?.Select(c => c.Id).ToList();
            roleViewModel.AvailableRoles = await editService.GetRoles(serviceSummaryViewModel.TFVersionViewModel.SelectedTFVersion.Version);

            roleViewModel.FromSummaryPage = fromSummaryPage;
            ViewBag.ServiceKey = serviceSummaryViewModel.ServiceKey;
            return View(roleViewModel);
        }

        [HttpPost("provider-roles")]
        public async Task<IActionResult> ProviderRoles(RoleViewModel roleViewModel)
        {
            bool fromSummaryPage = roleViewModel.FromSummaryPage;          
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            List<RoleDto> availableRoles = await editService.GetRoles(summaryViewModel.TFVersionViewModel.SelectedTFVersion.Version);
            roleViewModel.AvailableRoles = availableRoles;
            roleViewModel.SelectedRoleIds = roleViewModel.SelectedRoleIds ?? [];
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
        public  IActionResult GPG44Input(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

            return View(summaryViewModel);
        }

        [HttpPost("gpg44-input")]
        public  IActionResult GPG44Input(ServiceSummaryViewModel serviceSummaryViewModel)
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
        public IActionResult GPG45Input(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

            return View(summaryViewModel);
        }

        [HttpPost("gpg45-input")]
        public IActionResult GPG45Input(ServiceSummaryViewModel serviceSummaryViewModel)
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
        public IActionResult SupplementarySchemesInput(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            summaryViewModel.FromSummaryPage = fromSummaryPage;
            HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

            return View(summaryViewModel);
        }

        [HttpPost("supplementary-schemes-input")]
        public IActionResult SupplementarySchemesInput(ServiceSummaryViewModel viewModel)
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
            supplementarySchemeViewModel.SelectedSupplementarySchemeIds = supplementarySchemeViewModel.SelectedSupplementarySchemeIds ?? [];
            if (supplementarySchemeViewModel.SelectedSupplementarySchemeIds.Count > 0)
                summaryViewModel.SupplementarySchemeViewModel.SelectedSupplementarySchemes = availableSupplementarySchemes.Where(c => supplementarySchemeViewModel.SelectedSupplementarySchemeIds.Contains(c.Id)).ToList();
          

            if (ModelState.IsValid)
            {
                HttpContext?.Session.Set("ServiceSummary", summaryViewModel);

                return RedirectToMissingMappings(supplementarySchemeViewModel.FromSummaryPage);
              
            }
            else
            {
                return View("SupplementarySchemes", supplementarySchemeViewModel);
            }
        }
        #endregion

        #region Conformity Issue date
        [HttpGet("issue-date")]
        public IActionResult ConformityIssueDate(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            DateViewModel dateViewModel = new()
            {
                PropertyName = "ConformityIssueDate"
            };
            if (summaryViewModel.ConformityIssueDate != null)
            {
                dateViewModel = GetDayMonthYear(summaryViewModel.ConformityIssueDate);
            }
            dateViewModel.FromSummaryPage = fromSummaryPage;
            ViewBag.serviceKey = summaryViewModel.ServiceKey;

            return View(dateViewModel);
        }

        /// <summary>
        /// Updates confirmity issue date variable in session 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost("issue-date")]
        public IActionResult ConformityIssueDate(DateViewModel dateViewModel)
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
        public IActionResult ConformityExpiryDate(bool fromSummaryPage)
        {
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            DateViewModel dateViewModel = new()
            {
                PropertyName = "ConformityExpiryDate"
            };
            if (summaryViewModel.ConformityExpiryDate != null)
            {
                dateViewModel = GetDayMonthYear(summaryViewModel.ConformityExpiryDate);
            }
            dateViewModel.FromSummaryPage = fromSummaryPage;
            ViewBag.serviceKey = summaryViewModel.ServiceKey;
            return View(dateViewModel);
        }


        /// <summary>
        /// Updates confirmity issue expiry date variable in session 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost("expiry-date")]
        public  IActionResult ConformityExpiryDate(DateViewModel dateViewModel)
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
            SetRefererURL();
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();
            return View(summaryViewModel);
        }

        [HttpGet("summary-of-changes")]
        public async Task<IActionResult> ServiceDifference()
        {
            var userEmails = await userService.GetUserEmailsExcludingLoggedIn(UserEmail);          
            ServiceChangesViewModel changesViewModel = new();
            ServiceSummaryViewModel summaryViewModel = GetServiceSummary();

            ServiceDto previousData = await editService.GetService(summaryViewModel.ServiceId);
            changesViewModel.DSITUserEmails = string.Join(",", userEmails);

            ServiceDraftDto currentData = ViewModelHelper.MapToDraft(previousData, summaryViewModel);

            (changesViewModel.PreviousDataKeyValuePair, changesViewModel.CurrentDataKeyValuePair) = await editService.GetServiceKeyValue(currentData, previousData);

            TempData["changedService"] = JsonConvert.SerializeObject(currentData);

            return View(changesViewModel);
        }



        [HttpPost("summary-of-changes")]
        public async Task<IActionResult> SaveServiceDraft(ServiceChangesViewModel serviceChangesViewModel)
        {
            var serializedData = TempData["changedService"] as string;
            
            if (string.IsNullOrEmpty(serializedData))
                throw new InvalidOperationException("Service draft data is missing.");
            
            ServiceDraftDto? serviceDraft = JsonConvert.DeserializeObject<ServiceDraftDto>(serializedData);
            
            if (serviceDraft == null)
                throw new InvalidOperationException("Unable to deserialize service draft data.");
            
            List<string> dsitUserEmails = serviceChangesViewModel.DSITUserEmails.Split(',').ToList();
            GenericResponse genericResponse = await editService.SaveServiceDraft(serviceDraft, UserEmail, dsitUserEmails);

            if (!genericResponse.Success)
                throw new InvalidOperationException("Failed to save service draft.");
            
            return RedirectToAction("InformationSubmitted", new { serviceId = serviceDraft.serviceId });
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


        #region Resend service update request

        [HttpPost("resend-service-update-request")]
        public async Task<IActionResult> ResendServiceUpdateRequest(int serviceDraftId, int providerId)
        {
            var userEmails = await userService.GetUserEmailsExcludingLoggedIn(UserEmail);
            GenericResponse genericResponse = await editService.GenerateTokenAndSendServiceUpdateRequest(null, UserEmail, userEmails, serviceDraftId, true);
            if (genericResponse.Success)
            {
                ViewBag.ProviderId = providerId;
                return View("ResendEmailSuccess");
            }
            else
            {
                throw new InvalidOperationException("Failed to resend update request.");
            }
        }
        #endregion
        #region Private Methods
          
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
