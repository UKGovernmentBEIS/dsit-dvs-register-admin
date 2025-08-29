
using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRegManagementService
    {
        public Task<List<ProviderProfileDto>> GetProviders();
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<ServiceDto> GetServiceDetails(int serviceId);      
       
        public Task<List<ServiceDto>> GetServiceVersionList(int serviceKey);
        public Task<List<string>> GetCabEmailListForServices(List<int> serviceIds);
        public Task<PaginatedResult<ServiceDto>> GetServiceHistory(int pageNumber, string sort, string sortAction);


    }
}
