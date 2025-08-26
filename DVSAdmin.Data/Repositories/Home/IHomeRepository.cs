using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Models;

namespace DVSAdmin.Data.Repositories
{
    public interface IHomeRepository
    {
        public Task<PaginatedResult<Service>> GetPendingCertificateReviews(int pageNumber, string sort, string sortAction);
        public Task<PaginatedResult<Service>> GetPendingPrimaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction);
        public Task<PaginatedResult<Service>> GetPendingSecondaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction);
        public Task<PaginatedResult<Service>> GetPendingRequests(string loggedInUserEmail, int pageNumber, string sort, string sortAction);     
        public Task<Dictionary<string, int>> GetPendingCounts(string loggedInUserEmail);
        public Task<User> GetUserByEmail(string userEmail);
    }
}
