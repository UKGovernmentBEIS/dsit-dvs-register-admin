using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.JWT;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class EditService : IEditService
    {
        private readonly IEditRepository _editRepository;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IJwtService _jwtService;

        public EditService(IEditRepository editRepository, IMapper mapper, IEmailSender emailSender, IJwtService jwtService)
        {
            _editRepository = editRepository;
            _mapper = mapper;
            _emailSender  = emailSender;
            _jwtService = jwtService;

        }
        

        public async Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail)
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

            }
            return response;
        }

        public async Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail)
        {
            var draftEntity = _mapper.Map<ServiceDraft>(draftDto);
            
            var response = await _editRepository.SaveServiceDraft(draftEntity, loggedInUserEmail);
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
    }
}
