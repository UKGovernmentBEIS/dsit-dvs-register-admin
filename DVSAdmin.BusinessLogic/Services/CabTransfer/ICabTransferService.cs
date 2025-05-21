using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICabTransferService
    {
        public Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber, string searchText = "");
        public Task<GenericResponse> SaveCabTransferRequest(CabTransferRequestDto cabTransferRequestDto, string loggedInUserEmail);
        public Task<GenericResponse> CancelCabTransferRequest(int cantransferRequestId, string loggedInUserEmail);
    }
}
