using AutoMapper;
using DVSAdmin.BusinessLogic.Models.Draft;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Edit;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Services.Edit
{
    public class EditService : IEditService
    {
        private readonly EditRepository _editRepository;
        private readonly IMapper _mapper;

        public EditService(EditRepository editRepository, IMapper mapper)
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
    }
}
