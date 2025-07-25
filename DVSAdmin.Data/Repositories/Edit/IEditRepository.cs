using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IEditRepository
    {
       
        public Task<Service> GetService(int serviceId);
        public Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId);
        public Task<ProviderProfile> GetProviderDetails(int providerId);
        public Task<List<Role>> GetRoles(decimal tfVersion);
        public Task<List<QualityLevel>> QualityLevels();
        public Task<List<IdentityProfile>> GetIdentityProfiles();
        public Task<List<SupplementaryScheme>> GetSupplementarySchemes();
        public Task<ServiceDraft?> GetServiceDraft(int serviceId);

        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraft draft, string loggedInUserEmail);
        public Task<GenericResponse> SaveProviderDraftToken(ProviderDraftToken providerDraftToken, string loggedinUserEmail, int providerProfileId);
        public Task<GenericResponse> SaveServiceDraftToken(ServiceDraftToken serviceDraftToken, string loggedinUserEmail, int serviceId);
        public Task<List<Service>> GetPublishedUnderpinningServices(string searchText, int? currentSelectedServiceId);
        public Task<List<Service>> GetServicesWithManualUnderinningService(string searchText, int? currentSelectedServiceId);
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<ManualUnderPinningService> GetManualUnderPinningServiceDetails(int serviceId);

    }
}
