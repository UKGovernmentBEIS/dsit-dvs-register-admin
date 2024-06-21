
using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IRegManagementService
    {
        public Task<List<ProviderDto>> GetProviders();
        public Task<ProviderDto> GetProviderDetails(int providerId);
        public Task<ProviderDto> GetProviderWithServiceDeatils(int providerId);
    }
}
