using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICabTransferService
    {
        public Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber, string searchText = "");
        Task<IReadOnlyList<CabDto>> ListCabsExceptCurrentAsync(int serviceId);
        public Task<GenericResponse> SaveCabTransferRequest(CabTransferRequestDto cabTransferRequestDto, string serviceName, string providerName, string loggedInUserEmail);
        public Task<GenericResponse> CancelCabTransferRequest(int cabTransferRequestId, string serviceName, string providerName, int toCabId, string loggedInUserEmail);
        public Task<ServiceDto> GetServiceDetails(int serviceId);
        public Task<CabTransferRequestDto> GetCabTransferDetails(int serviceId);
    }
}
