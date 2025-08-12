using DVSAdmin.Data.Entities;
using static DVSAdmin.Data.Repositories.CabTransferRepository;

namespace DVSAdmin.Data.Repositories
{
    public interface IHomeRepository
    {
        public Task<PaginatedResult<Service>> GetServices(int pageNumber, string sort, string sortAction, string openTask);
        public Task<Dictionary<string, int>> GetPendingCounts();
        public Task<User> GetUserByEmail(string userEmail);
    }
}
