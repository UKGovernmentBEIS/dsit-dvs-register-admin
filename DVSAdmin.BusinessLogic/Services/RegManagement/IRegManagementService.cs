
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRegManagementService
    {
        public Task<List<ProviderProfileDto>> GetProviders();
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<ServiceDto> GetServiceDetails(int serviceId);
        public Task<ProviderProfileDto> GetProviderWithServiceDetails(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string loggedInUserEmail);
       
     
    }
}
