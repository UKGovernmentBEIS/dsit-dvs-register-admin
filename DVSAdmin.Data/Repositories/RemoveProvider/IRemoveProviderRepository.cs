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
        public Task<GenericResponse> UpdateProviderStatus(int providerProfileId, ProviderStatusEnum providerStatus, string loggedInUserEmail, EventTypeEnum eventType, TeamEnum team = TeamEnum.DSIT);
        public Task<GenericResponse> SaveRemoveProviderToken(RemoveProviderToken removeProviderToken, TeamEnum team, EventTypeEnum eventType, string loggedinUserEmail, bool isResend);
        public Task<GenericResponse> RemoveProviderRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, RemovalReasonsEnum? reason);
        public Task<GenericResponse> RemoveServiceRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, ServiceRemovalReasonEnum? serviceRemovalReason);
        public Task<ProviderProfile> GetProviderAndServices(int providerId);
        public Task<GenericResponse> CancelRemoveServiceRequest(int providerProfileId, int serviceId, string loggedInUserEmail);
    }
}
