using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class EditService : IEditService
    {
        private readonly IEditRepository _editRepository;
        private readonly IMapper _mapper;

        public EditService(IEditRepository editRepository, IMapper mapper)
        {
            _editRepository = editRepository;
            _mapper = mapper;
        }
        

        public async Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail)
        {
            var draftEntity = _mapper.Map<ProviderProfileDraft>(draftDto);
            
            var response = await _editRepository.SaveProviderDraft(draftEntity, loggedInUserEmail);
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
    }
}
