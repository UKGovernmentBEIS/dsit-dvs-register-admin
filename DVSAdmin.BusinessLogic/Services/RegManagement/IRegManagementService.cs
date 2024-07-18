
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRegManagementService
    {
        public Task<List<ProviderDto>> GetProviders();
        public Task<ProviderDto> GetProviderDetails(int providerId);
        public Task<ProviderDto> GetProviderWithServiceDeatils(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string userEmail);
    }
}
