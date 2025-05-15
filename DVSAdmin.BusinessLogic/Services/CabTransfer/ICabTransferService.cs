using DVSAdmin.BusinessLogic.Models;
using static DVSAdmin.BusinessLogic.Services.CabTransfer.CabTransferService;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface ICabTransferService
    {
        public Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber);
    }
}
