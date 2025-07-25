using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services
{
    public class EditService : IEditService
    {
        private readonly IEditRepository _editRepository;
        private readonly IRemoveProviderService _removeProviderService;
        private readonly IMapper _mapper;
        private readonly EditEmailSender _emailSender;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ICabTransferRepository _cabTransferRepository;

        public EditService(IEditRepository editRepository, IRemoveProviderService removeProviderService, IMapper mapper, EditEmailSender emailSender,
            IJwtService jwtService, IConfiguration configuration, ICabTransferRepository cabTransferRepository)
        {
            _removeProviderService = removeProviderService;
            _editRepository = editRepository;
            _mapper = mapper;
            _emailSender  = emailSender;
            _jwtService = jwtService;
            _configuration = configuration;
            _cabTransferRepository = cabTransferRepository;

        }
        

        public async Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail, List<string> dsitUserEmails)
        {
            var draftEntity = _mapper.Map<ProviderProfileDraft>(draftDto);            
            var response = await _editRepository.SaveProviderDraft(draftEntity, loggedInUserEmail);
            if(response.Success) 
            {        
                
                TokenDetails tokenDetails = _jwtService.GenerateToken("DSIT", draftDto.ProviderProfileId);
                ProviderDraftToken providerDraftToken = new()
                {
                    ProviderProfileDraftId = response.InstanceId,
                    Token = tokenDetails.Token,
                    TokenId = tokenDetails.TokenId,                   
                    CreatedTime = DateTime.UtcNow
                };
                response = await _editRepository.SaveProviderDraftToken(providerDraftToken, loggedInUserEmail, draftDto.ProviderProfileId);
                if(response.Success)
                {
                    ProviderProfile providerProfile = await _editRepository.GetProviderDetails(draftDto.ProviderProfileId);
                    ProviderProfileDto providerProfileDto = _mapper.Map<ProviderProfileDto>(providerProfile);
                    //57/DSIT/2i edit update request - provider
                    string dsit2iCheckLink = _configuration["DvsRegisterLink"] + "update-request/provider-changes?token=" + tokenDetails.Token;
                    var (previous, current) = GetProviderKeyValue(draftDto, providerProfileDto);
                    string currentData = Helper.ConcatenateKeyValuePairs(current);
                    string previousData = Helper.ConcatenateKeyValuePairs(previous);
                    foreach(var email in dsitUserEmails)
                    {
                        await _emailSender.ProviderEditRequest(email, email, providerProfileDto.RegisteredName, currentData, previousData, dsit2iCheckLink);
                    }

                    //To do 59/DSIT/Update request submitted - provider// send email to loggedInUserEmail
                    await _emailSender.ProviderEditRequestConfirmation(loggedInUserEmail, loggedInUserEmail, providerProfileDto.RegisteredName, currentData, previousData);

                }


            }
            return response;
        }




        public (Dictionary<string, List<string>>, Dictionary<string, List<string>>) GetProviderKeyValue(ProviderProfileDraftDto currentData, ProviderProfileDto previousData)
        {
            var previousDataDictionary = new Dictionary<string, List<string>>();

            var currentDataDictionary = new Dictionary<string, List<string>>();

            if (currentData.RegisteredName != null)
            {
                previousDataDictionary.Add("Registered name", [previousData.RegisteredName]);
                currentDataDictionary.Add("Registered name", [currentData.RegisteredName]);
            }

            if (currentData.TradingName != null)
            {
                previousDataDictionary.Add("Trading name", [previousData.TradingName]);
                currentDataDictionary.Add("Trading name", [currentData.TradingName]);
            }
           
            if (currentData.PrimaryContactFullName != null)
            {
                previousDataDictionary.Add("Primary contact full name", [previousData.PrimaryContactFullName]);
                currentDataDictionary.Add("Primary contact full name", [currentData.PrimaryContactFullName]);
            }
            if (currentData.PrimaryContactEmail != null)
            {
                previousDataDictionary.Add("Primary contact email", [previousData.PrimaryContactEmail]);
                currentDataDictionary.Add("Primary contact email", [currentData.PrimaryContactEmail]);
            }
            if (currentData.PrimaryContactJobTitle != null)
            {
                previousDataDictionary.Add("Primary contact job title", [previousData.PrimaryContactJobTitle]);
                currentDataDictionary.Add("primary contact job title", [currentData.PrimaryContactJobTitle]);
            }
            if (currentData.PrimaryContactTelephoneNumber != null)
            {
                previousDataDictionary.Add("Primary contact telephone number", [previousData.PrimaryContactTelephoneNumber]);
                currentDataDictionary.Add("Primary contact telephone number", [currentData.PrimaryContactTelephoneNumber]);
            }

            if (currentData.SecondaryContactFullName != null)
            {
                previousDataDictionary.Add("Secondary contact full name", [previousData.SecondaryContactFullName]);
                currentDataDictionary.Add("Secondary contact full name", [currentData.SecondaryContactFullName]);
            }
            if (currentData.SecondaryContactEmail != null)
            {
                previousDataDictionary.Add("Secondary contact email", [previousData.SecondaryContactEmail]);
                currentDataDictionary.Add("Secondary contact email", [currentData.SecondaryContactEmail]);
            }
            if (currentData.SecondaryContactJobTitle != null)
            {
                previousDataDictionary.Add("Secondary contact job title", [previousData.SecondaryContactJobTitle]);
                currentDataDictionary.Add("Secondary contact job title", [currentData.SecondaryContactJobTitle]);
            }
            if (currentData.SecondaryContactTelephoneNumber != null)
            {
                previousDataDictionary.Add("Secondary contact telephone number", [previousData.SecondaryContactTelephoneNumber]);
                currentDataDictionary.Add("Secondary contact telephone number", [currentData.SecondaryContactTelephoneNumber]);
            }
            
            if (currentData.ProviderWebsiteAddress != null)
            {
                previousDataDictionary.Add("Provider website address", [previousData.ProviderWebsiteAddress]);
                currentDataDictionary.Add("Provider website address", [currentData.ProviderWebsiteAddress]);
            }
          
            if (currentData.PublicContactEmail != null)
            {
                previousDataDictionary.Add("Public contact email", [previousData.PublicContactEmail]);
                currentDataDictionary.Add("Public contact email", [currentData.PublicContactEmail]);
            }
         
            if (currentData.ProviderTelephoneNumber != null)
            {
                previousDataDictionary.Add("Provider telephone number", [previousData.ProviderTelephoneNumber]);
                currentDataDictionary.Add("Provider telephone number", [currentData.ProviderTelephoneNumber]);
            }

            return (previousDataDictionary,currentDataDictionary);
        }

        



        public async Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail, List<string> dsitUserEmails)
        {
            var draftEntity = _mapper.Map<ServiceDraft>(draftDto);
            
            var response = await _editRepository.SaveServiceDraft(draftEntity, loggedInUserEmail);
            GenericResponse genericResponse = await _removeProviderService.UpdateProviderStatusByStatusPriority(draftDto.ProviderProfileId, TeamEnum.DSIT.ToString(), EventTypeEnum.DSITEditService, TeamEnum.DSIT);

            if (response.Success && genericResponse.Success)
            {
                response = await GenerateTokenAndSendServiceUpdateRequest(draftDto, loggedInUserEmail, dsitUserEmails, response.InstanceId, false);
            }
            return response;
        }

        public async Task<GenericResponse> GenerateTokenAndSendServiceUpdateRequest(ServiceDraftDto? draftDto, string loggedInUserEmail, List<string> dsitUserEmails, int serviceDraftId, bool isResend)
        {

            GenericResponse response = new();
            if (draftDto == null && isResend)
            {
                var serviceDraft = await _editRepository.GetServiceDraft(serviceDraftId);
                draftDto = _mapper.Map<ServiceDraftDto>(serviceDraft);

            }
            TokenDetails tokenDetails = _jwtService.GenerateToken("DSIT", draftDto.ProviderProfileId, draftDto.serviceId.ToString());
            ServiceDraftToken serviceDraftToken = new()
            {
                ServiceDraftId = serviceDraftId,
                Token = tokenDetails.Token,
                TokenId = tokenDetails.TokenId                
            };
           
            response = await _editRepository.SaveServiceDraftToken(serviceDraftToken, loggedInUserEmail, draftDto.serviceId);
            if (response.Success)
            {               

                //56/DSIT/2i edit update request - service

                Service service = await _editRepository.GetService(draftDto.serviceId);
                ServiceDto serviceDto = _mapper.Map<ServiceDto>(service);
                string dsit2iCheckLink = _configuration["DvsRegisterLink"] + "update-request/service-changes?token=" + tokenDetails.Token;
                var (previous, current) = await GetServiceKeyValue(draftDto, serviceDto);
                string currentData = Helper.ConcatenateKeyValuePairs(current);
                string previousData = Helper.ConcatenateKeyValuePairs(previous);
                foreach (var email in dsitUserEmails)
                {
                    await _emailSender.ServiceEditRequest(email, email, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, currentData, previousData, dsit2iCheckLink);
                }

                //To do 58/DSIT/Update request submitted - service// send email to loggedInUserEmail
                await _emailSender.ServiceEditRequestConfirmation(loggedInUserEmail, loggedInUserEmail, serviceDto.Provider.RegisteredName, serviceDto.ServiceName, currentData, previousData);

            }

            return response;
        }

        public async Task<ServiceDto> GetService(int serviceId)
        {
            var serviceDetails = await _editRepository.GetService(serviceId);
            ServiceDto serviceDto = _mapper.Map<ServiceDto>(serviceDetails);
            return serviceDto;
        }

        public async Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId)
        {
            return await _editRepository.CheckProviderRegisteredNameExists(registeredName, providerId);

        }

        public async Task<ProviderProfileDto> GetProviderDetails(int providerId)
        {
            var provider = await _editRepository.GetProviderDetails(providerId);
            ProviderProfileDto providerProfileDto = _mapper.Map<ProviderProfileDto>(provider);
            return providerProfileDto;
        }
        public async Task<List<RoleDto>> GetRoles(decimal tfVersion)
        {
            var list = await _editRepository.GetRoles(tfVersion);
            return _mapper.Map<List<RoleDto>>(list);
        }
        public async Task<List<QualityLevelDto>> GetQualitylevels()
        {
            var list = await _editRepository.QualityLevels();
            return _mapper.Map<List<QualityLevelDto>>(list);
        }
        public async Task<List<SupplementarySchemeDto>> GetSupplementarySchemes()
        {
            var list = await _editRepository.GetSupplementarySchemes();
            return _mapper.Map<List<SupplementarySchemeDto>>(list);
        }
        public async Task<List<IdentityProfileDto>> GetIdentityProfiles()
        {
            var list = await _editRepository.GetIdentityProfiles();
            return _mapper.Map<List<IdentityProfileDto>>(list);
        }

        public async Task<IReadOnlyList<CabDto>> GetAllCabs()
        {

            var allCabs = await _cabTransferRepository.GetAllCabsAsync();
            List<CabDto> cabDtos = _mapper.Map<List<CabDto>>(allCabs);
            return cabDtos;
        }

        public async Task<List<ServiceDto>> GetPublishedUnderpinningServices(string searchText, int? currentSelectedServiceId)
        {
            var services = await _editRepository.GetPublishedUnderpinningServices(searchText, currentSelectedServiceId);
            var serviceDtos = _mapper.Map<List<ServiceDto>>(services);

            return serviceDtos;
        }

        public async Task<List<ServiceDto>> GetServicesWithManualUnderinningService(string searchText, int? currentSelectedServiceId)
        {
            var services = await _editRepository.GetServicesWithManualUnderinningService(searchText, currentSelectedServiceId);
            var serviceDtos = _mapper.Map<List<ServiceDto>>(services);

            return serviceDtos;
        }

        public async Task<(Dictionary<string, List<string>>, Dictionary<string, List<string>>)> GetServiceKeyValue(ServiceDraftDto currentData, ServiceDto previousData)
        {

            var currentDataDictionary = new Dictionary<string, List<string>>();
            var previousDataDictionary = new Dictionary<string, List<string>>();
            GetServiceKeyValueMappings(currentData, previousData, currentDataDictionary, previousDataDictionary);

            if (previousData.TrustFrameworkVersion.Version == Constants.TFVersion0_4)
            {
                GetServiceKeyValueForSchemeMappingsTFV0_4(currentData, previousData, currentDataDictionary, previousDataDictionary);

                await GetServiceKeyValueMappingsForUnderpinningServiceTFV0_4(currentData, previousData, currentDataDictionary, previousDataDictionary);

            }


            return (previousDataDictionary, currentDataDictionary);
        }

        private static void GetServiceKeyValueMappings(ServiceDraftDto currentData, ServiceDto previousData, Dictionary<string, List<string>> currentDataDictionary, Dictionary<string, List<string>> previousDataDictionary)
        {
            if (currentData.CompanyAddress != null)
            {
                previousDataDictionary.Add(Constants.RegisteredAddress, [previousData.CompanyAddress]);
                currentDataDictionary.Add(Constants.RegisteredAddress, [currentData.CompanyAddress]);
            }

            if (currentData.ServiceName != null)
            {
                previousDataDictionary.Add(Constants.ServiceName, [previousData.ServiceName]);
                currentDataDictionary.Add(Constants.ServiceName, [currentData.ServiceName]);
            }
            if (currentData.ServiceRoleMappingDraft.Count > 0)
            {
                var roles = previousData.ServiceRoleMapping.Select(item => item.Role.RoleName).ToList();
                previousDataDictionary.Add(Constants.Roles, roles);

                var currentRoles = currentData.ServiceRoleMappingDraft.Select(item => item.Role.RoleName).ToList();
                currentDataDictionary.Add(Constants.Roles, currentRoles);


            }
            var protectionExists = currentData.ServiceQualityLevelMappingDraft.Any(m => m.QualityLevel.QualityType == QualityTypeEnum.Protection);
            var authenticationExists = currentData.ServiceQualityLevelMappingDraft.Any(m => m.QualityLevel.QualityType == QualityTypeEnum.Authentication);

            if (protectionExists || currentData.HasGPG44 == false)
            {
                var protectionLevels = previousData.ServiceQualityLevelMapping?
                    .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Protection)
                    .Select(item => item.QualityLevel.Level)
                    .ToList();

                var currentProtectionLevels = currentData.ServiceQualityLevelMappingDraft?
                    .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Protection)
                    .Select(item => item.QualityLevel.Level)
                    .ToList();

                previousDataDictionary.Add(Constants.GPG44Protection, protectionLevels != null && protectionLevels.Count > 0 ? protectionLevels : new List<string> { @Constants.NullFieldsDisplay });
                currentDataDictionary.Add(Constants.GPG44Protection, currentProtectionLevels != null && currentProtectionLevels.Count > 0 ? currentProtectionLevels : new List<string> { @Constants.NullFieldsDisplay });
            }

            if (authenticationExists || currentData.HasGPG44 == false)
            {
                var authenticationLevels = previousData.ServiceQualityLevelMapping?
                    .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                    .Select(item => item.QualityLevel.Level)
                    .ToList();

                var currentAuthenticationLevels = currentData.ServiceQualityLevelMappingDraft?
                    .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Authentication)
                    .Select(item => item.QualityLevel.Level)
                    .ToList();

                previousDataDictionary.Add(Constants.GPG44Authentication, authenticationLevels != null && authenticationLevels.Count > 0 ? authenticationLevels : new List<string> { @Constants.NullFieldsDisplay });
                currentDataDictionary.Add(Constants.GPG44Authentication, currentAuthenticationLevels != null && currentAuthenticationLevels.Count > 0 ? currentAuthenticationLevels : new List<string> { @Constants.NullFieldsDisplay });
            }

            #region GPG45 Identity profile
            if (currentData.ServiceIdentityProfileMappingDraft.Count > 0 || currentData.HasGPG45 == false)
            {
                var identityProfiles = previousData.ServiceIdentityProfileMapping?.Select(item => item.IdentityProfile.IdentityProfileName).ToList();
                var currentIdentityProfiles = currentData.ServiceIdentityProfileMappingDraft?.Select(item => item.IdentityProfile.IdentityProfileName).ToList();
                if (identityProfiles != null && identityProfiles.Count > 0)
                {
                    previousDataDictionary.Add(Constants.GPG45IdentityProfiles, identityProfiles);

                }
                else
                {
                    previousDataDictionary.Add(Constants.GPG45IdentityProfiles, [@Constants.NullFieldsDisplay]);
                }

                if (currentIdentityProfiles != null && currentIdentityProfiles.Count > 0)
                {
                    currentDataDictionary.Add(Constants.GPG45IdentityProfiles, currentIdentityProfiles);
                }
                else
                {
                    currentDataDictionary.Add(Constants.GPG45IdentityProfiles, [@Constants.NullFieldsDisplay]);
                }
            }

            #endregion


            #region Scheme

            if (currentData.ServiceSupSchemeMappingDraft.Count > 0 || currentData.HasSupplementarySchemes == false)
            {
                var supplementarySchemes = previousData.ServiceSupSchemeMapping?.OrderBy(x => x.SupplementarySchemeId).Select(item => item.SupplementaryScheme.SchemeName).ToList();
                var currentSupplementarySchemes = currentData.ServiceSupSchemeMappingDraft?.OrderBy(x => x.SupplementarySchemeId).Select(item => item.SupplementaryScheme.SchemeName).ToList();
                bool areSame = supplementarySchemes != null && currentSupplementarySchemes != null &&
                supplementarySchemes.SequenceEqual(currentSupplementarySchemes);


                if (!areSame)
                {
                    if (supplementarySchemes != null && supplementarySchemes.Count > 0)
                    {
                        previousDataDictionary.Add(Constants.SupplementaryCodes, supplementarySchemes);
                    }
                    else
                    {
                        previousDataDictionary.Add(Constants.SupplementaryCodes, [@Constants.NullFieldsDisplay]);
                    }

                    if (currentSupplementarySchemes != null && currentSupplementarySchemes.Count > 0)
                    {
                        currentDataDictionary.Add(Constants.SupplementaryCodes, currentSupplementarySchemes);
                    }
                    else
                    {
                        currentDataDictionary.Add(Constants.SupplementaryCodes, [@Constants.NullFieldsDisplay]);
                    }
                }


            }



            #endregion


            if (currentData.ConformityIssueDate != null)
            {
                previousDataDictionary.Add("Issue date", [Helper.GetLocalDateTime(previousData.ConformityIssueDate, "dd MMMM yyyy")]);
                currentDataDictionary.Add("Issue date", [Helper.GetLocalDateTime(currentData.ConformityIssueDate, "dd MMMM yyyy")]);
            }

            if (currentData.ConformityExpiryDate != null)
            {
                previousDataDictionary.Add("Expiry date", [Helper.GetLocalDateTime(previousData.ConformityExpiryDate, "dd MMMM yyyy")]);
                currentDataDictionary.Add("Expiry date", [Helper.GetLocalDateTime(currentData.ConformityExpiryDate, "dd MMMM yyyy")]);
            }
        }

        private async Task GetServiceKeyValueMappingsForUnderpinningServiceTFV0_4(ServiceDraftDto currentData, ServiceDto previousData, Dictionary<string, List<string>> currentDataDictionary, Dictionary<string, List<string>> previousDataDictionary)
        {
            // previous selected service: published under pinning
            if (previousData.IsUnderPinningServicePublished == true && previousData.UnderPinningServiceId != null)
            {
              

               

                if (currentData.IsUnderpinningServicePublished == true && currentData.UnderPinningServiceId != null && currentData.UnderPinningServiceId!= previousData.UnderPinningServiceId) //current data is selected from list of manual services 
                {
                    await PopulatePreviousPublishedUnderPinningService(previousDataDictionary, (int) previousData.UnderPinningServiceId);

                    Service currentServiceDto = await _editRepository.GetServiceDetails((int)currentData.UnderPinningServiceId);
                    currentDataDictionary.Add(Constants.UnderpinningServiceName, [currentServiceDto.ServiceName]);
                    currentDataDictionary.Add(Constants.UnderpinningProviderName, [currentServiceDto.Provider.RegisteredName]);
                    currentDataDictionary.Add(Constants.CabOfUnderpinningService, [currentServiceDto.CabUser.Cab.CabName]);
                    currentDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(currentServiceDto.ConformityExpiryDate, "dd MMMM yyyy")]);

                }
                else if (currentData.IsUnderpinningServicePublished == false && currentData.ManualUnderPinningServiceId != null) //current data is selected from list of manual services 
                {
                    await PopulatePreviousPublishedUnderPinningService(previousDataDictionary, (int)previousData.UnderPinningServiceId);
                    ManualUnderPinningService currentSelectedManualService = await _editRepository.GetManualUnderPinningServiceDetails((int)currentData.ManualUnderPinningServiceId);
                    GetKeyValueForSelectedManualUnderpinningService(currentData, currentDataDictionary, currentSelectedManualService);

                }
                else if (currentData.IsUnderpinningServicePublished == false && currentData.ManualUnderPinningServiceId == null && currentData.ManualUnderPinningServiceDraft != null)
                {
                    await PopulatePreviousPublishedUnderPinningService(previousDataDictionary, (int)previousData.UnderPinningServiceId);

                    GetKeyValueForManualUnderpinningDraft(currentData, currentDataDictionary);
                }
            }
            else if (previousData.IsUnderPinningServicePublished == false && previousData.ManualUnderPinningServiceId != null)
            {
               

                //current is a published service
                if (currentData.IsUnderpinningServicePublished == true && currentData.UnderPinningServiceId != null)
                {
                    await PopulatePreviousManualUnderpinningService(previousDataDictionary, (int)previousData.ManualUnderPinningServiceId);

                    Service currentServiceDto = await _editRepository.GetServiceDetails((int)currentData.UnderPinningServiceId);
                    currentDataDictionary.Add(Constants.UnderpinningServiceName, [currentServiceDto.ServiceName]);
                    currentDataDictionary.Add(Constants.UnderpinningProviderName, [currentServiceDto.Provider.RegisteredName]);
                    currentDataDictionary.Add(Constants.CabOfUnderpinningService, [currentServiceDto.CabUser.Cab.CabName]);
                    currentDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(currentServiceDto.ConformityExpiryDate, "dd MMMM yyyy")]);
                }
                else if (currentData.IsUnderpinningServicePublished == false && currentData.ManualUnderPinningServiceId != null ) // manual selected service
                {
                    await PopulatePreviousManualUnderpinningService(previousDataDictionary, (int)previousData.ManualUnderPinningServiceId);

                    ManualUnderPinningService currentSelectedManualService = await _editRepository.GetManualUnderPinningServiceDetails((int)currentData.ManualUnderPinningServiceId);
                    GetKeyValueForSelectedManualUnderpinningService(currentData, currentDataDictionary, currentSelectedManualService);
                }
                else if (currentData.IsUnderpinningServicePublished == false && currentData.ManualUnderPinningServiceId == null && currentData.ManualUnderPinningServiceDraft!=null) // manualluy enetered service
                {
                    await PopulatePreviousManualUnderpinningService(previousDataDictionary, (int)previousData.ManualUnderPinningServiceId);
                    GetKeyValueForManualUnderpinningDraft(currentData, currentDataDictionary);
                }


            }
        }

        private async Task PopulatePreviousManualUnderpinningService( Dictionary<string, List<string>> previousDataDictionary, int manualUnderPinningServiceId)
        {
            ManualUnderPinningService previousManualUnderPinningService = await _editRepository.GetManualUnderPinningServiceDetails(manualUnderPinningServiceId);
            // previous underpinning is a manual service
            previousDataDictionary.Add(Constants.UnderpinningServiceName, [previousManualUnderPinningService.ServiceName]);
            previousDataDictionary.Add(Constants.UnderpinningProviderName, [previousManualUnderPinningService.ProviderName]);
            previousDataDictionary.Add(Constants.CabOfUnderpinningService, [previousManualUnderPinningService.Cab.CabName]);
            previousDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(previousManualUnderPinningService.CertificateExpiryDate, "dd MMMM yyyy")]);
        }

        private async Task PopulatePreviousPublishedUnderPinningService(Dictionary<string, List<string>> previousDataDictionary, int underPinningServiceId)
        {
            Service previousServiceDto = await _editRepository.GetServiceDetails(underPinningServiceId);
            previousDataDictionary.Add(Constants.UnderpinningServiceName, [previousServiceDto.ServiceName]);
            previousDataDictionary.Add(Constants.UnderpinningProviderName, [previousServiceDto.Provider.RegisteredName]);
            previousDataDictionary.Add(Constants.CabOfUnderpinningService, [previousServiceDto.CabUser.Cab.CabName]);
            previousDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(previousServiceDto.ConformityExpiryDate, "dd MMMM yyyy")]);
        }

        private static void GetKeyValueForSelectedManualUnderpinningService(ServiceDraftDto currentData, Dictionary<string, List<string>> currentDataDictionary, ManualUnderPinningService currentSelectedManualService)
        {
            if (currentData.ManualUnderPinningServiceDraft.ServiceName != currentSelectedManualService.ServiceName)
            {
                currentDataDictionary.Add(Constants.UnderpinningServiceName, [currentData.ManualUnderPinningServiceDraft.ServiceName]);
            }
            else
            {
                currentDataDictionary.Add(Constants.UnderpinningServiceName, [currentSelectedManualService.ServiceName]);
            }
            if (currentData.ManualUnderPinningServiceDraft.ProviderName != currentSelectedManualService.ProviderName)
            {
                currentDataDictionary.Add(Constants.UnderpinningProviderName, [currentData.ManualUnderPinningServiceDraft.ProviderName]);
            }
            else
            {
                currentDataDictionary.Add(Constants.UnderpinningProviderName, [currentSelectedManualService.ProviderName]);
            }
            if (currentData.ManualUnderPinningServiceDraft.CabId != currentSelectedManualService.CabId)
            {
                currentDataDictionary.Add(Constants.CabOfUnderpinningService, [currentData.ManualUnderPinningServiceDraft.SelectedCabName]);
            }
            else
            {
                currentDataDictionary.Add(Constants.CabOfUnderpinningService, [currentSelectedManualService.Cab.CabName]);
            }
            if (currentData.ManualUnderPinningServiceDraft.CertificateExpiryDate != currentSelectedManualService.CertificateExpiryDate)
            {
                currentDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(currentData.ManualUnderPinningServiceDraft.CertificateExpiryDate, "dd MMMM yyyy")]);
            }
            else
            {
                currentDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(currentSelectedManualService.CertificateExpiryDate, "dd MMMM yyyy")]);
            }
        }

        private static void GetKeyValueForManualUnderpinningDraft(ServiceDraftDto currentData, Dictionary<string, List<string>> currentDataDictionary)
        {

            // new manually entered service
            currentDataDictionary.Add(Constants.UnderpinningServiceName, [currentData.ManualUnderPinningServiceDraft.ServiceName]);
            currentDataDictionary.Add(Constants.UnderpinningProviderName, [currentData.ManualUnderPinningServiceDraft.ProviderName]);
            currentDataDictionary.Add(Constants.CabOfUnderpinningService, [currentData.ManualUnderPinningServiceDraft.SelectedCabName]);
            currentDataDictionary.Add(Constants.UnderpiningExpiryDate, [Helper.GetLocalDateTime(currentData.ManualUnderPinningServiceDraft.CertificateExpiryDate, "dd MMMM yyyy")]);
        }

   

        private static void GetServiceKeyValueForSchemeMappingsTFV0_4(ServiceDraftDto currentData, ServiceDto previousData, Dictionary<string, List<string>> currentDataDictionary, Dictionary<string, List<string>> previousDataDictionary)
        {



            if (currentData.ServiceSupSchemeMappingDraft.Count>0 && currentData.ServiceSupSchemeMappingDraft.Count < previousData?.ServiceSupSchemeMapping?.Count)
            {

                foreach (var schemeMapping in previousData.ServiceSupSchemeMapping)
                {
                    var previousSchemeMapping = previousData.ServiceSupSchemeMapping.Where(x => x.SupplementarySchemeId == schemeMapping.SupplementarySchemeId).FirstOrDefault();
                    var currentMappingDraft = currentData.ServiceSupSchemeMappingDraft .Where(mapping => mapping.SupplementarySchemeId == schemeMapping.SupplementarySchemeId).FirstOrDefault();

                    if(previousSchemeMapping != null)
                    {
                        var previousSchemeGpg45Mappings = previousSchemeMapping?.SchemeGPG45Mapping?.Select(x => x.IdentityProfile.IdentityProfileName)?.ToList();                    

                        if (currentMappingDraft == null)//removed  scheme
                        {
                            previousDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles,
                            previousSchemeGpg45Mappings != null && previousSchemeGpg45Mappings.Count > 0 ? previousSchemeGpg45Mappings : [@Constants.NullFieldsDisplay]);

                            currentDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles, [@Constants.NullFieldsDisplay]);
                        }
                        else if(currentMappingDraft != null && currentMappingDraft.SchemeGPG45MappingDraft != null && currentMappingDraft.SchemeGPG45MappingDraft.Count > 0) //modified scheme
                        {
                            previousDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles,
                            previousSchemeGpg45Mappings != null && previousSchemeGpg45Mappings.Count > 0 ? previousSchemeGpg45Mappings : [@Constants.NullFieldsDisplay]);

                            var currentSchemeGpg45Mappings = currentMappingDraft?.SchemeGPG45MappingDraft?.Select(x => x.IdentityProfile.IdentityProfileName).ToList();
                            currentDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles, currentSchemeGpg45Mappings);
                        }


                        var previousSchemeGpg44AuthenticationMappings = previousSchemeMapping?.SchemeGPG44Mapping?.Where(x => x.QualityLevel.QualityType == QualityTypeEnum.Authentication).Select(x => x.QualityLevel.Level)?.ToList();
                        var previousSchemeGpg44ProtectionMappings = previousSchemeMapping?.SchemeGPG44Mapping?.Where(x => x.QualityLevel.QualityType == QualityTypeEnum.Protection).Select(x => x.QualityLevel.Level)?.ToList();
                                            



                        if (currentMappingDraft == null)//removed  scheme
                        {
                            previousDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection,
                            previousSchemeGpg45Mappings != null && previousSchemeGpg45Mappings.Count > 0 ? previousSchemeGpg44ProtectionMappings : [@Constants.NullFieldsDisplay]);

                            previousDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication,
                            previousSchemeGpg45Mappings != null && previousSchemeGpg45Mappings.Count > 0 ? previousSchemeGpg44AuthenticationMappings : [@Constants.NullFieldsDisplay]);

                            currentDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection, [@Constants.NullFieldsDisplay]);
                            currentDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication, [@Constants.NullFieldsDisplay]);
                        }
                        else if (currentMappingDraft != null && currentMappingDraft.SchemeGPG45MappingDraft != null && currentMappingDraft.SchemeGPG45MappingDraft.Count > 0 
                            || currentMappingDraft.HasGpg44Mapping == false) //modified scheme
                        {
                            var currentSchemeGpg44Protection = currentMappingDraft?.SchemeGPG44MappingDraft?
                           .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Protection).Select(x => x.QualityLevel.Level).ToList();

                            var currentSchemeGpg44Authentication = currentMappingDraft?.SchemeGPG44MappingDraft?
                            .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Authentication).Select(x => x.QualityLevel.Level).ToList();


                            previousDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection,
                            previousSchemeGpg45Mappings != null && previousSchemeGpg45Mappings.Count > 0 ? previousSchemeGpg44ProtectionMappings : [@Constants.NullFieldsDisplay]);

                            previousDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication,
                            previousSchemeGpg45Mappings != null && previousSchemeGpg45Mappings.Count > 0 ? previousSchemeGpg44AuthenticationMappings : [@Constants.NullFieldsDisplay]);


                            currentDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection,
                            currentSchemeGpg44Protection!=null && currentSchemeGpg44Protection.Count>0? currentSchemeGpg44Protection
                            : [@Constants.NullFieldsDisplay]);

                            currentDataDictionary.Add(schemeMapping.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication,
                            currentSchemeGpg44Protection != null && currentSchemeGpg44Authentication.Count > 0 ? currentSchemeGpg44Authentication
                            : [@Constants.NullFieldsDisplay]);
                        }

                    }
                    
                   
                }

            }

            else
            {
                foreach (var schemeMappingDraft in currentData.ServiceSupSchemeMappingDraft)
                {
                    var previousSchemeMapping = previousData.ServiceSupSchemeMapping.Where(x => x.SupplementarySchemeId == schemeMappingDraft.SupplementarySchemeId).FirstOrDefault();

                    if (schemeMappingDraft.SchemeGPG45MappingDraft != null && schemeMappingDraft.SchemeGPG45MappingDraft.Count > 0)
                    {
                      

                        if (previousSchemeMapping != null)
                        {
                            var schemeGpg45Mappings = previousSchemeMapping.SchemeGPG45Mapping.Select(x => x.IdentityProfile.IdentityProfileName).ToList();
                            previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles, schemeGpg45Mappings);
                        }
                        else
                        {
                            previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles, [@Constants.NullFieldsDisplay]);
                        }
                        var currentSchemeGpg45Mappings = schemeMappingDraft?.SchemeGPG45MappingDraft?.Select(x => x.IdentityProfile.IdentityProfileName).ToList();

                        if (currentSchemeGpg45Mappings != null && currentSchemeGpg45Mappings.Count > 0)
                        {
                            currentDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles, currentSchemeGpg45Mappings);
                        }
                        else
                        {
                            currentDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG45IdentityProfiles, [@Constants.NullFieldsDisplay]);
                            
                        }
                    }


                    if (schemeMappingDraft.SchemeGPG44MappingDraft != null && schemeMappingDraft.SchemeGPG44MappingDraft.Count > 0 
                        || (schemeMappingDraft.HasGpg44Mapping != previousSchemeMapping?.HasGpg44Mapping))
                    {

                        
                        if (previousSchemeMapping != null)
                            {
                                var previousSchemeGpg44Protection = previousSchemeMapping.SchemeGPG44Mapping?
                              .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Protection).Select(x => x.QualityLevel.Level).ToList();

                                var previousSchemeGpg44Authentication = previousSchemeMapping.SchemeGPG44Mapping?
                                 .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Authentication).Select(x => x.QualityLevel.Level).ToList();


                                if (previousSchemeGpg44Authentication != null && previousSchemeGpg44Authentication.Count > 0 && previousSchemeGpg44Protection != null && previousSchemeGpg44Protection.Count > 0)
                                {
                                    previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection, previousSchemeGpg44Protection);
                                    previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication, previousSchemeGpg44Authentication);

                                }
                                else
                                {
                                    previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection, [@Constants.NullFieldsDisplay]);
                                    previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication, [@Constants.NullFieldsDisplay]);

                                }

                            }
                            else
                            {
                                previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection, [@Constants.NullFieldsDisplay]);
                                previousDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication, [@Constants.NullFieldsDisplay]);

                            }

                            var currentSchemeGpg44Protection = schemeMappingDraft?.SchemeGPG44MappingDraft?
                            .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Protection).Select(x => x.QualityLevel.Level).ToList();

                            var currentSchemeGpg44Authentication = schemeMappingDraft?.SchemeGPG44MappingDraft?
                            .Where(m => m.QualityLevel.QualityType == QualityTypeEnum.Authentication).Select(x => x.QualityLevel.Level).ToList();

                            if (currentSchemeGpg44Protection != null && currentSchemeGpg44Protection.Count > 0 && currentSchemeGpg44Authentication != null
                                && currentSchemeGpg44Authentication.Count > 0)
                            {
                                currentDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection, currentSchemeGpg44Protection);
                                currentDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication, currentSchemeGpg44Authentication);

                            }
                            else
                            {
                                currentDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Protection, [@Constants.NullFieldsDisplay]);
                                currentDataDictionary.Add(schemeMappingDraft.SupplementaryScheme.SchemeName + ":" + Constants.GPG44Authentication, [@Constants.NullFieldsDisplay]);


                            } 
                        
                    }
                }
            }
         
        }


    }
}
