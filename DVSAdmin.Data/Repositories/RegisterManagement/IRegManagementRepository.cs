using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;


namespace DVSAdmin.Data.Repositories
{
    public interface IRegManagementRepository
    {
        public Task<List<ProviderProfile>> GetProviders();
        public Task<ProviderProfile> GetProviderDetails(int providerId);
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<ProviderProfile> GetProviderWithServiceDetails(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string loggedInUserEmail);
        public Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog, string loggedInUserEmail, List<int> serviceIds);       
        public Task<List<Service>> GetPublishedServices();
        public Task<List<Service>> GetServiceVersionList(int serviceKey);
        public Task<List<Service>> GetServiceListByProvider(int providerId);
        public Task<ProviderProfile> GetProviderDetailsWithOutReviewDetails(int providerId);
        public Task<List<string>> GetCabEmailListForServices(List<int> serviceIds);




    }
}
