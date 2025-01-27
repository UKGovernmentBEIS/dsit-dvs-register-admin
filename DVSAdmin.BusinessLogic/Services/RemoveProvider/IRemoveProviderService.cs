using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRemoveProviderService
    {
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<ServiceDto> GetServiceDetails(int serviceId);
        public Task<GenericResponse> RemoveServiceRequestByCab(int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string>? dsitUserEmails);
    }
}
