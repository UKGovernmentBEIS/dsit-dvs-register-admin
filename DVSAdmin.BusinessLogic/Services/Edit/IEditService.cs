using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IEditService
    {
        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail);
        public Task<ServiceDto> GetService(int serviceId);
        public Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId);
        public Task<ProviderProfileDto> GetProviderDeatils(int providerId);
    }
}
