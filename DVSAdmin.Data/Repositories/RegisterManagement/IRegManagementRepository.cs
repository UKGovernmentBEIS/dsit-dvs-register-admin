using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;


namespace DVSAdmin.Data.Repositories
{
    public interface IRegManagementRepository
    {
        public Task<List<ProviderProfile>> GetProviders();
        public Task<ProviderProfile> GetProviderDetails(int providerId);
        public Task<Service> GetServiceDetails(int serviceId);      
        public Task<List<Service>> GetPublishedServices();
        public Task<List<Service>> GetServiceVersionList(int serviceKey);       
        public Task<List<string>> GetCabEmailListForServices(List<int> serviceIds);


    }
}
