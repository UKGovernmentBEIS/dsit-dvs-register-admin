using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
    public interface IRegManagementRepository
    {
        public Task<List<Provider>> GetProviders();
        public Task<Provider> GetProviderDetails(int providerId);
    }
}
