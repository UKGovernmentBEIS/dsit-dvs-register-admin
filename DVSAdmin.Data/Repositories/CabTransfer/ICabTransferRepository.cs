using DVSAdmin.Data.Entities;
using static DVSAdmin.Data.Repositories.CabTransferRepository;

namespace DVSAdmin.Data.Repositories
{
    public interface ICabTransferRepository
    {
        public Task<PaginatedResult<Service>> GetServices(int pageNumber);
    }
}
