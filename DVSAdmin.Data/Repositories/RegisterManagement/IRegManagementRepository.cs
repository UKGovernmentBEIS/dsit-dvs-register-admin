using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;


namespace DVSAdmin.Data.Repositories
{
    public interface IRegManagementRepository
    {
        public Task<List<ProviderProfile>> GetProviders();
        public Task<ProviderProfile> GetProviderDetails(int providerId);
        public Task<List<RemovalReasons>> GetRemovalReasons();
        public Task<ProviderProfile> GetProviderWithServiceDetails(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, ServiceStatusEnum serviceStatus, string loggedInUserEmail);
        public Task<GenericResponse> UpdateProviderStatus(int providerId, ProviderStatusEnum providerStatus,string loggedInUserEmail);
        public Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog, string loggedInUserEmail);
        public Task<GenericResponse> UpdateRemovalStatus(int providerProfileId, List<int> serviceIds, string reason, string loggedInUserEmail);
        public  Task<GenericResponse> SaveRemoveProviderToken(RemoveProviderToken removeProviderToken, TeamEnum team, EventTypeEnum eventType, string loggedinUserEmail);
    }
}
