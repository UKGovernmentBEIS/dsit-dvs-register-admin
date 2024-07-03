using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IRegManagementRepository
    {
        public Task<List<Provider>> GetProviders();
        public Task<Provider> GetProviderDetails(int providerId);
        public Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string userEmail, CertificateInfoStatusEnum certificateInfoStatus);
        public Task<GenericResponse> UpdateProviderStatus(int providerId, ProviderStatusEnum providerStatus);
        public Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog);
    }
}
