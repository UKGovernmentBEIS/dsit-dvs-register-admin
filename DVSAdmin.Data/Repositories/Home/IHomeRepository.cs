using DVSAdmin.Data.Entities;
using static DVSAdmin.Data.Repositories.CabTransferRepository;

namespace DVSAdmin.Data.Repositories
{
    public interface IHomeRepository
    {
        public Task<PaginatedResult<Service>> GetServices(string loggedInUserEmail,int pageNumber, string sort, string sortAction, string openTask);
        public Task<Dictionary<string, int>> GetPendingCounts(string loggedInUserEmail);
        public Task<User> GetUserByEmail(string userEmail);
    }
}
