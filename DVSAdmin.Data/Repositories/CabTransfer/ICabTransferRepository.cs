using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using static DVSAdmin.Data.Repositories.CabTransferRepository;

namespace DVSAdmin.Data.Repositories
{
    public interface ICabTransferRepository
    {
        public Task<PaginatedResult<Service>> GetServices(int pageNumber, string searchText = "");
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<CabTransferRequest> GetCabTransferDetails(int serviceId);
        public Task<GenericResponse> SaveCabTransferRequest(CabTransferRequest cabTransferRequest, string loggedInUserEmail);
        public Task<GenericResponse> CancelCabTransferRequest(int cabtransferRequestId, string loggedInUserEmail);
    }
}
