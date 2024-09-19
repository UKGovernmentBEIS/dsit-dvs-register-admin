using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IPublicInterestCheckRepository
    {
        public Task<List<Service>> GetPICheckList();
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheck publicInterestCheck, ReviewTypeEnum reviewType);
        public  Task<Service> GetServiceDetailsWithMappings(int serviceId);
        public Task<GenericResponse> SavePICheckLog(PICheckLogs pICheck);
        public Task<List<Service>> GetServiceList(int providerId);
        public Task<GenericResponse> UpdateServiceAndProviderStatus(int serviceId, ProviderStatusEnum providerStatus);
    }
}
