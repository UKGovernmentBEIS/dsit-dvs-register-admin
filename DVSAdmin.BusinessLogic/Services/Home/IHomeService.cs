using DVSAdmin.BusinessLogic.Models;
namespace DVSAdmin.BusinessLogic.Services
{
    public interface IHomeService
    {
        public Task<PaginatedResult<ServiceDto>> GetPendingCertificateReviews(int pageNumber, string sort, string sortAction);
        public Task<PaginatedResult<ServiceDto>> GetPendingPrimaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction);
        public Task<PaginatedResult<ServiceDto>> GetPendingSecondaryChecks(string loggedInUserEmail, int pageNumber, string sort, string sortAction);
        public Task<PaginatedResult<ServiceDto>> GetPendingRequests(string loggedInUserEmail, int pageNumber, string sort, string sortAction);      
        public Task<Dictionary<string, int>> GetPendingCounts(string loggedInUserEmail);
        public Task<UserDto> GetUserByEmail(string userEmail);
    }
}
