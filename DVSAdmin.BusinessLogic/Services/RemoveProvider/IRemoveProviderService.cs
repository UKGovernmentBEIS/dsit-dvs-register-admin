using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRemoveProviderService
    {
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<ServiceDto> GetServiceDetails(int serviceId);
        public Task<GenericResponse> RemoveServiceRequestByCab(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> cabEmails, List<string>? dsitUserEmails);
        public Task<GenericResponse> RemoveProviderRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, RemovalReasonsEnum? reason);
        public Task<GenericResponse> RemoveServiceRequest(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, ServiceRemovalReasonEnum? serviceRemovalReason);
        public Task<GenericResponse> UpdateProviderStatusByStatusPriority(int providerProfileId, string loggedInUserEmail, EventTypeEnum eventType, TeamEnum team = TeamEnum.DSIT);        
        public Task<GenericResponse> CancelRemoveServiceRequest(int providerProfileId, int serviceId, string loggedInUserEmail);
        public Task<GenericResponse> GenerateTokenAndSendServiceRemoval(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, TeamEnum team ,ServiceRemovalReasonEnum? serviceRemovalReason, bool isResend);
    }
}
