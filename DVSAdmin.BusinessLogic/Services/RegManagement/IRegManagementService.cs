
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRegManagementService
    {
        public Task<List<ProviderProfileDto>> GetProviders();
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<List<RemovalReasonDto>> GetRemovalReasons();
        public Task<ProviderProfileDto> GetProviderWithServiceDeatils(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string loggedInUserEmail);

        public Task<GenericResponse> PublishRemovalReason(int providerId, string reason, string loggedInUserEmail);
    }
}
