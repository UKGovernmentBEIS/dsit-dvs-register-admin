using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IEditRepository
    {
        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraft draft, string loggedInUserEmail);
        public Task<Service> GetService(int serviceId);
        public Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId);
        public Task<ProviderProfile> GetProviderDetails(int providerId);
        public Task<List<Role>> GetRoles();
        public Task<List<QualityLevel>> QualityLevels();
        public Task<List<IdentityProfile>> GetIdentityProfiles();
        public Task<List<SupplementaryScheme>> GetSupplementarySchemes();
        public Task<GenericResponse> SaveProviderDraftToken(ProviderDraftToken providerDraftToken, string loggedinUserEmail);

    }
}
