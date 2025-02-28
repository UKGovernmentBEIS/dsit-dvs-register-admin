using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace DVSAdmin.BusinessLogic.Services
{
    public class EditService : IEditService
    {
        private readonly IEditRepository _editRepository;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public EditService(IEditRepository editRepository, IMapper mapper, IEmailSender emailSender, IJwtService jwtService, IConfiguration configuration)
        {
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
                // to do send email
                TokenDetails tokenDetails = _jwtService.GenerateToken("DSIT");
                ProviderDraftToken providerDraftToken = new()
                {
                    ProviderProfileDraftId = response.InstanceId,
                    Token = tokenDetails.Token,
                    TokenId = tokenDetails.TokenId,                   
                    CreatedTime = DateTime.UtcNow
                };
                response = await _editRepository.SaveProviderDraftToken(providerDraftToken, loggedInUserEmail);
                if(response.Success)
                {
                    //to do email
                }


            }
            return response;
        }

        public async Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail, List<string> dsitUserEmails)
        {
            var draftEntity = _mapper.Map<ServiceDraft>(draftDto);
            
            var response = await _editRepository.SaveServiceDraft(draftEntity, loggedInUserEmail);

            if (response.Success)
            {
                // to do send email
                TokenDetails tokenDetails = _jwtService.GenerateToken("DSIT");
                ServiceDraftToken serviceDraftToken = new()
                {
                    ServiceDraftId = response.InstanceId,
                    Token = tokenDetails.Token,
                    TokenId = tokenDetails.TokenId,
                    CreatedTime = DateTime.UtcNow
                };
                response = await _editRepository.SaveServiceDraftToken(serviceDraftToken, loggedInUserEmail);
                if (response.Success)
                {
                    //to do email
                }
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

        public async Task<ProviderProfileDto> GetProviderDeatils(int providerId)
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

    }
}
