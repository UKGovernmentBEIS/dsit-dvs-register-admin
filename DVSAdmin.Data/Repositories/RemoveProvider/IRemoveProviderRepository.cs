using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;


namespace DVSAdmin.Data.Repositories
{
    public interface IRemoveProviderRepository
    {
        public Task<ProviderProfile> GetProviderDetails(int providerId);
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<GenericResponse> RemoveServiceRequestByCab(int providerProfileId, List<int> serviceIds, string loggedInUserEmail);
        public Task<GenericResponse> UpdateProviderStatus(int providerProfileId, ProviderStatusEnum providerStatus, string loggedInUserEmail, EventTypeEnum eventType);

    }
}
