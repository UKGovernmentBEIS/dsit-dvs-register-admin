
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSRegister.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRegManagementService
    {
        public Task<List<ProviderProfileDto>> GetProviders();
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<ProviderProfileDto> GetProviderWithServiceDeatils(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string loggedInUserEmail);
        public Task<GenericResponse> UpdateRemovalStatus(EventTypeEnum eventType,  int providerProfileId, List<int> serviceIds, string loggedInUserEmail, List<string> dsitUserEmails, RemovalReasonsEnum? reason, ServiceRemovalReasonEnum? serviceRemovalReason);
    }
}
