using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Models;

namespace DVSAdmin.Data.Repositories
{
    public interface ICabTransferRepository
    {
        public Task<PaginatedResult<Service>> GetServices(int pageNumber, string sort, string sortAction, string searchText = "");
        public Task<List<CabUser>> GetActiveCabUsers(int cabId);
        Task<List<Cab>> GetAllCabsAsync();
        Task<Cab> GetCabByIdAsync(int cabId);
        public Task<Service> GetServiceDetails(int serviceId);
        public Task<CabTransferRequest> GetCabTransferDetails(int serviceId);

        public Task<GenericResponse> SaveCabTransferRequest(CabTransferRequest cabTransferRequest,
            string loggedInUserEmail);

        public Task<GenericResponse> CancelCabTransferRequest(int cabtransferRequestId, string loggedInUserEmail);
    }
}