using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSAdmin.Validations;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Mvc;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    [Route("edit-provider")]
    public class EditProviderController : Controller
    {
        private readonly IEditService editService;       
        private readonly IUserService userService;
        private string userEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;
        public EditProviderController(IEditService editService, IUserService userService)
        {
            this.editService = editService;            
            this.userService = userService;

        }


        [HttpGet("change-provider-details")]
        public async Task<IActionResult> ProfileSummary(int providerId, bool isEditPage)
        {
            ViewBag.isEditPage = isEditPage;
           
            if(isEditPage)
            {
                ProviderProfileDto providerDto = await editService.GetProviderDeatils(providerId);
                ProfileSummaryViewModel profileSummaryViewModel = MapDtoToViewModel(providerDto);
                HttpContext?.Session.Set("ProfileSummary", profileSummaryViewModel);
                return View(profileSummaryViewModel);
            }
            else
            {
                
                ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
                return View(profileSummaryViewModel);
            }
        }



        #region Registered Name

        [HttpGet("reg-name")]
        public IActionResult RegisteredName(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();           
            return View("RegisteredName", profileSummaryViewModel);
        }

        [HttpPost("reg-name")]
        public async Task<IActionResult> SaveRegisteredName(ProfileSummaryViewModel profileSummaryViewModel)
        {           

            if (!string.IsNullOrEmpty(profileSummaryViewModel.RegisteredName))
            {
                bool registeredNameExist =
                    await editService.CheckProviderRegisteredNameExists(profileSummaryViewModel.RegisteredName,profileSummaryViewModel.ProviderProfileId);
                if (registeredNameExist)
                {
                    ModelState.AddModelError("RegisteredName", Constants.RegisteredNameExistsError);
                }
            }

            if (ModelState["RegisteredName"].Errors.Count == 0)
            {
                ProfileSummaryViewModel profileSummary = GetProfileSummary();
                profileSummary.RegisteredName = profileSummaryViewModel.RegisteredName;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("RegisteredName", profileSummaryViewModel);
            }
        }

        #endregion


        #region Trading Name

        [HttpGet("trading-name")]
        public IActionResult TradingName(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            return View("TradingName", profileSummaryViewModel);
        }

        [HttpPost("trading-name")]
        public IActionResult SaveTradingName(ProfileSummaryViewModel profileSummaryViewModel)
        {
           
            if (ModelState["TradingName"].Errors.Count == 0)
            {
                ProfileSummaryViewModel profileSummary = GetProfileSummary();
                profileSummary.TradingName = profileSummaryViewModel.TradingName;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("TradingName", profileSummaryViewModel);
            }
        }

        #endregion

        #region Primary Contact

        [HttpGet("primary-contact-information")]
        public IActionResult PrimaryContact(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            ViewBag.hasParentCompany = profileSummaryViewModel.HasParentCompany;
            return View(profileSummaryViewModel.PrimaryContact);
        }

        [HttpPost("primary-contact-information")]
        public IActionResult SavePrimaryContact(PrimaryContactViewModel primaryContactViewModel)
        {
            ProfileSummaryViewModel profileSummary = GetProfileSummary();
            ValidationHelper.ValidateDuplicateFields(
                ModelState,
                primaryContactViewModel.PrimaryContactEmail,
                profileSummary.SecondaryContact?.SecondaryContactEmail,
                new ValidationHelper.FieldComparisonConfig(
                    "PrimaryContactEmail",
                    "SecondaryContactEmail",
                    "Email address of secondary contact cannot be the same as primary contact"
                )
            );

            ValidationHelper.ValidateDuplicateFields(
                ModelState,
                primaryValue: primaryContactViewModel.PrimaryContactTelephoneNumber,
                secondaryValue: profileSummary.SecondaryContact?.SecondaryContactTelephoneNumber,
                new ValidationHelper.FieldComparisonConfig(
                    "PrimaryContactTelephoneNumber",
                    "SecondaryContactTelephoneNumber",
                    "Telephone number of secondary contact cannot be the same as primary contact"
                )
            );

            if (ModelState.IsValid)
            {
                profileSummary.PrimaryContact = primaryContactViewModel;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("PrimaryContact", primaryContactViewModel);
            }
        }

        #endregion

        #region Secondary Contact

        [HttpGet("secondary-contact-information")]
        public IActionResult SecondaryContact(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            return View(profileSummaryViewModel.SecondaryContact);
        }


        [HttpPost("secondary-contact-information")]
        public IActionResult SaveSecondaryContact(SecondaryContactViewModel secondaryContactViewModel)
        { 

            ProfileSummaryViewModel profileSummary = GetProfileSummary();
            ValidationHelper.ValidateDuplicateFields(
                ModelState,
                primaryValue: profileSummary.PrimaryContact?.PrimaryContactEmail,
                secondaryValue: secondaryContactViewModel.SecondaryContactEmail,
                new ValidationHelper.FieldComparisonConfig(
                    "PrimaryContactEmail",
                    "SecondaryContactEmail",
                    "Email address of secondary contact cannot be the same as primary contact"
                )
            );

            ValidationHelper.ValidateDuplicateFields(
                ModelState,
                primaryValue: profileSummary.PrimaryContact?.PrimaryContactTelephoneNumber,
                secondaryValue: secondaryContactViewModel.SecondaryContactTelephoneNumber,
                new ValidationHelper.FieldComparisonConfig(
                    "PrimaryContactTelephoneNumber",
                    "SecondaryContactTelephoneNumber",
                    "Telephone number of secondary contact cannot be the same as primary contact"
                )
            );

            if (ModelState.IsValid)
            {
                profileSummary.SecondaryContact = secondaryContactViewModel;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("SecondaryContact", secondaryContactViewModel);
            }
        }

        #region Public contact email

        [HttpGet("public-email")]
        public IActionResult PublicContactEmail(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            return View("PublicContactEmail", profileSummaryViewModel);
        }

        [HttpPost("public-email")]
        public IActionResult SavePublicContactEmail(ProfileSummaryViewModel profileSummaryViewModel)
        {
            
            if (ModelState["PublicContactEmail"].Errors.Count == 0)
            {
                ProfileSummaryViewModel profileSummary = GetProfileSummary();
                profileSummary.PublicContactEmail = profileSummaryViewModel.PublicContactEmail;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("PublicContactEmail", profileSummaryViewModel);
            }
        }

        #endregion

        #region Telephone number

        [HttpGet("public-telephone")]
        public IActionResult TelephoneNumber(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            return View("TelephoneNumber", profileSummaryViewModel);
        }

        [HttpPost("public-telephone")]
        public IActionResult SaveTelephoneNumber(ProfileSummaryViewModel profileSummaryViewModel)
        {
           
            if (ModelState["ProviderTelephoneNumber"]?.Errors.Count == 0)
            {
                ProfileSummaryViewModel profileSummary = GetProfileSummary();
                profileSummary.ProviderTelephoneNumber = profileSummaryViewModel.ProviderTelephoneNumber;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("TelephoneNumber", profileSummaryViewModel);
            }
        }

        #endregion

        #region Website address

        [HttpGet("public-website")]
        public IActionResult WebsiteAddress(bool fromSummaryPage)
        {
            ViewBag.fromSummaryPage = fromSummaryPage;
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            return View("WebsiteAddress", profileSummaryViewModel);
        }

        [HttpPost("public-website")]
        public IActionResult SaveWebsiteAddress(ProfileSummaryViewModel profileSummaryViewModel)
        {           
            if (ModelState["ProviderWebsiteAddress"]?.Errors.Count == 0)
            {
                ProfileSummaryViewModel profileSummary = GetProfileSummary();
                profileSummary.ProviderWebsiteAddress = profileSummaryViewModel.ProviderWebsiteAddress;
                HttpContext?.Session.Set("ProfileSummary", profileSummary);
                return RedirectToAction("ProfileSummary");
            }
            else
            {
                return View("WebsiteAddress", profileSummaryViewModel);
            }
        }

        #endregion



        [HttpGet("summary-of-changes")]
        public async Task<IActionResult> ProviderDifference()
        {
            ProviderChangesViewModel changesViewModel = new();
            ProfileSummaryViewModel profileSummaryViewModel = GetProfileSummary();
            ProviderProfileDto providerProfileDto = await editService.GetProviderDeatils(profileSummaryViewModel.ProviderProfileId);
            ProviderProfileDraftDto providerProfileDraftDto = CreateDraft(providerProfileDto, profileSummaryViewModel);
            List<string> dsitEmails = await userService.GetUserEmailsExcludingLoggedIn(userEmail);
            changesViewModel.DSITUserEmails = string.Join(",", dsitEmails);            
            changesViewModel.CurrentProvider = providerProfileDto;
            changesViewModel.ChangedProvider = providerProfileDraftDto;
            return View(changesViewModel);
        }


        [HttpPost("summary-of-changes")]
        public async Task<IActionResult> SaveProviderDraft(ProviderChangesViewModel providerChangesViewModel)
        {
            List<string> dsitUserEmails = providerChangesViewModel.DSITUserEmails.Split(',').ToList();
            if (providerChangesViewModel != null && providerChangesViewModel.ChangedProvider != null)
            {
                GenericResponse genericResponse = await editService.SaveProviderDraft(providerChangesViewModel.ChangedProvider, userEmail,dsitUserEmails);
                if(genericResponse.Success)
                {
                    return RedirectToAction("InformationSubmitted", new { providerId = providerChangesViewModel.ChangedProvider.ProviderProfileId });
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
        [HttpGet("provider-submitted")]
        public async Task<IActionResult> InformationSubmitted(int providerId)
        {
            HttpContext?.Session.Remove("ProfileSummary");
            ProviderProfileDto providerDto = await editService.GetProviderDeatils(providerId);
            return View(providerDto);
           

        }

        #endregion

        #region Private Methods

        private ProfileSummaryViewModel GetProfileSummary()
        {
            ProfileSummaryViewModel model = HttpContext?.Session.Get<ProfileSummaryViewModel>("ProfileSummary") ??
                                            new ProfileSummaryViewModel
                                            {
                                                PrimaryContact = new PrimaryContactViewModel(),
                                                SecondaryContact = new SecondaryContactViewModel()
                                            };
            return model;
        }


        private ProfileSummaryViewModel MapDtoToViewModel(ProviderProfileDto providerDto)
        {
            ProfileSummaryViewModel profileSummaryViewModel = new();

            profileSummaryViewModel.RegisteredName = providerDto.RegisteredName;
            profileSummaryViewModel.TradingName = providerDto.TradingName;
            profileSummaryViewModel.HasRegistrationNumber = providerDto.HasRegistrationNumber;
            profileSummaryViewModel.CompanyRegistrationNumber = providerDto.CompanyRegistrationNumber;
            profileSummaryViewModel.DUNSNumber = providerDto.DUNSNumber;
            profileSummaryViewModel.HasParentCompany = providerDto.HasParentCompany;
            profileSummaryViewModel.ParentCompanyRegisteredName = providerDto.ParentCompanyRegisteredName;
            profileSummaryViewModel.ParentCompanyLocation = providerDto.ParentCompanyLocation;
            profileSummaryViewModel.PrimaryContact = new PrimaryContactViewModel
            {
                PrimaryContactFullName = providerDto.PrimaryContactFullName,
                PrimaryContactJobTitle = providerDto.PrimaryContactJobTitle,
                PrimaryContactEmail = providerDto.PrimaryContactEmail,
                PrimaryContactTelephoneNumber = providerDto.PrimaryContactTelephoneNumber,
                ProviderId = providerDto.Id
             };

            profileSummaryViewModel.SecondaryContact = new SecondaryContactViewModel
            {
                SecondaryContactFullName = providerDto.SecondaryContactFullName,
                SecondaryContactJobTitle = providerDto.SecondaryContactJobTitle,
                SecondaryContactEmail = providerDto.SecondaryContactEmail,
                SecondaryContactTelephoneNumber = providerDto.SecondaryContactTelephoneNumber,
                ProviderId = providerDto.Id
            };
            
            profileSummaryViewModel.PublicContactEmail = providerDto.PublicContactEmail;
            profileSummaryViewModel.ProviderTelephoneNumber = providerDto.ProviderTelephoneNumber;
            profileSummaryViewModel.ProviderWebsiteAddress = providerDto.ProviderWebsiteAddress;
            profileSummaryViewModel.ProviderProfileId = providerDto.Id;
            profileSummaryViewModel.IsEditable  = providerDto.ProviderStatus == ProviderStatusEnum.ReadyToPublish || providerDto.ProviderStatus == ProviderStatusEnum.ReadyToPublishNext || 
                providerDto.ProviderStatus == ProviderStatusEnum.Published;
            return profileSummaryViewModel;
        }


        private ProviderProfileDraftDto CreateDraft(ProviderProfileDto existingProvider, ProfileSummaryViewModel updatedService)
        {
           

            var draft = new ProviderProfileDraftDto
            {
                ProviderProfileId = existingProvider.Id,
                PreviousProviderStatus = existingProvider.ProviderStatus               
            };

            draft.RegisteredName = updatedService.RegisteredName!=existingProvider.RegisteredName? updatedService.RegisteredName:null;           
            draft.TradingName = updatedService.TradingName != existingProvider.TradingName
            ? (updatedService.TradingName ??"-")
            : null;
            draft.PrimaryContactFullName = updatedService?.PrimaryContact?.PrimaryContactFullName != existingProvider.PrimaryContactFullName ? updatedService?.PrimaryContact?.PrimaryContactFullName : null;
            draft.PrimaryContactEmail = updatedService?.PrimaryContact?.PrimaryContactEmail != existingProvider.PrimaryContactEmail ? updatedService?.PrimaryContact?.PrimaryContactEmail : null;
            draft.PrimaryContactJobTitle = updatedService?.PrimaryContact?.PrimaryContactJobTitle != existingProvider.PrimaryContactJobTitle ? updatedService?.PrimaryContact?.PrimaryContactJobTitle: null;
            draft.PrimaryContactTelephoneNumber = updatedService?.PrimaryContact?.PrimaryContactTelephoneNumber != existingProvider.PrimaryContactTelephoneNumber ? updatedService?.PrimaryContact?.PrimaryContactTelephoneNumber : null;

            draft.SecondaryContactFullName = updatedService?.SecondaryContact?.SecondaryContactFullName != existingProvider.SecondaryContactFullName ? updatedService?.SecondaryContact?.SecondaryContactFullName : null;
            draft.SecondaryContactEmail = updatedService?.SecondaryContact?.SecondaryContactEmail != existingProvider.SecondaryContactEmail ? updatedService?.SecondaryContact?.SecondaryContactEmail : null;
            draft.SecondaryContactJobTitle = updatedService?.SecondaryContact?.SecondaryContactJobTitle != existingProvider.SecondaryContactJobTitle ? updatedService?.SecondaryContact?.SecondaryContactJobTitle : null;
            draft.SecondaryContactTelephoneNumber = updatedService?.SecondaryContact?.SecondaryContactTelephoneNumber != existingProvider.SecondaryContactTelephoneNumber ? updatedService?.SecondaryContact?.SecondaryContactTelephoneNumber : null;

            draft.ProviderWebsiteAddress = updatedService.ProviderWebsiteAddress != existingProvider.ProviderWebsiteAddress ? updatedService.ProviderWebsiteAddress : null;
            draft.PublicContactEmail = updatedService.PublicContactEmail != existingProvider.PublicContactEmail ? updatedService.PublicContactEmail : null;
            draft.ProviderTelephoneNumber = updatedService.ProviderTelephoneNumber != existingProvider.ProviderTelephoneNumber
          ? (updatedService.ProviderTelephoneNumber ?? "-")
          : null;

            return draft;
        }
            #endregion
        }
}
