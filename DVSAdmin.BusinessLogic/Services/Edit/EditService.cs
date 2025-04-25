using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models.Enums;

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

        public EditService(IEditRepository editRepository, IRemoveProviderService removeProviderService, IMapper mapper, EditEmailSender emailSender, IJwtService jwtService, IConfiguration configuration)
        {
            _removeProviderService = removeProviderService;
            _editRepository = editRepository;
            _mapper = mapper;
            _emailSender  = emailSender;
            _jwtService = jwtService;
            _configuration = configuration;

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
            GenericResponse genericResponse = await _removeProviderService.UpdateProviderStatusByStatusPriority(draftDto.ProviderProfileId, TeamEnum.CronJob.ToString(), EventTypeEnum.RemovedByCronJob, TeamEnum.CronJob);

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
                var (previous, current) = GetServiceKeyValue(draftDto, serviceDto);
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
        public async Task<List<RoleDto>> GetRoles()
        {
            var list = await _editRepository.GetRoles();
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


        public (Dictionary<string, List<string>>, Dictionary<string, List<string>>) GetServiceKeyValue(ServiceDraftDto currentData, ServiceDto previousData)
        {          

            var currentDataDictionary = new Dictionary<string, List<string>>();
            var previousDataDictionary = new Dictionary<string, List<string>>();

            if (currentData.CompanyAddress != null)
            {
                previousDataDictionary.Add("Registered address", [previousData.CompanyAddress]);
                currentDataDictionary.Add("Registered address", [currentData.CompanyAddress]);
            }

            if (currentData.ServiceName != null)
            {
                previousDataDictionary.Add("Service name", [previousData.ServiceName]);
                currentDataDictionary.Add("Service name", [currentData.ServiceName]);
            }



            if (currentData.ServiceRoleMappingDraft.Count > 0)
            {
                var roles = previousData.ServiceRoleMapping.Select(item => item.Role.RoleName).ToList();
                previousDataDictionary.Add("Roles certified against", roles);

                var currentRoles = currentData.ServiceRoleMappingDraft.Select(item => item.Role.RoleName).ToList();
                currentDataDictionary.Add("Roles certified against", currentRoles);


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

                previousDataDictionary.Add("GPG44 level of protection", protectionLevels != null && protectionLevels.Count > 0 ? protectionLevels : new List<string> { @Constants.NullFieldsDisplay });
                currentDataDictionary.Add("GPG44 level of protection", currentProtectionLevels != null && currentProtectionLevels.Count > 0 ? currentProtectionLevels : new List<string> { @Constants.NullFieldsDisplay });
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

                previousDataDictionary.Add("GPG44 quality of authentication", authenticationLevels != null && authenticationLevels.Count > 0 ? authenticationLevels : new List<string> { @Constants.NullFieldsDisplay });
                currentDataDictionary.Add("GPG44 quality of authentication", currentAuthenticationLevels != null && currentAuthenticationLevels.Count > 0 ? currentAuthenticationLevels : new List<string> { @Constants.NullFieldsDisplay });
            }

            #region GPG45 Identity profile
            if (currentData.ServiceIdentityProfileMappingDraft.Count > 0 || currentData.HasGPG45 == false)
            {
                var identityProfiles = previousData.ServiceIdentityProfileMapping?.Select(item => item.IdentityProfile.IdentityProfileName).ToList();
                var currentIdentityProfiles = currentData.ServiceIdentityProfileMappingDraft?.Select(item => item.IdentityProfile.IdentityProfileName).ToList();
                if (identityProfiles != null && identityProfiles.Count > 0)
                {
                    previousDataDictionary.Add("GPG45 identity profiles", identityProfiles);
                   
                }
                else
                {
                    previousDataDictionary.Add("GPG45 identity profiles", [@Constants.NullFieldsDisplay]);
                }

                if (currentIdentityProfiles != null && currentIdentityProfiles.Count > 0)
                {
                    currentDataDictionary.Add("GPG45 identity profiles", currentIdentityProfiles);
                }
                else
                {
                    currentDataDictionary.Add("GPG45 identity profiles", [@Constants.NullFieldsDisplay]);
                }
            }
           
            #endregion


            #region Scheme

            if (currentData.ServiceSupSchemeMappingDraft.Count > 0 || currentData.HasSupplementarySchemes == false)
            {
                var supplementarySchemes = previousData.ServiceSupSchemeMapping?.Select(item => item.SupplementaryScheme.SchemeName).ToList();
                var currentSupplementarySchemes = currentData.ServiceSupSchemeMappingDraft?.Select(item => item.SupplementaryScheme.SchemeName).ToList();
                if (supplementarySchemes != null && supplementarySchemes.Count > 0)
                {
                    previousDataDictionary.Add("Supplementary Codes", supplementarySchemes);
                }
                else
                {
                    previousDataDictionary.Add("Supplementary Codes", [@Constants.NullFieldsDisplay]);
                }

                if (currentSupplementarySchemes != null && currentSupplementarySchemes.Count > 0)
                {
                    currentDataDictionary.Add("Supplementary Codes", currentSupplementarySchemes);
                }
                else
                {
                    currentDataDictionary.Add("Supplementary Codes", [@Constants.NullFieldsDisplay]);
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
        
            return (previousDataDictionary, currentDataDictionary);
        }

    }
}
