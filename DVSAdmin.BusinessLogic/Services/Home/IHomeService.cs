using DVSAdmin.BusinessLogic.Models;
namespace DVSAdmin.BusinessLogic.Services
{
    public interface IHomeService
    {
        public Task<PaginatedResult<ServiceDto>> GetServices(string loggedInUserEmail, int pageNumber, string sort, string sortAction, string openTask);
        public Task<Dictionary<string, int>> GetPendingCounts(string loggedInUserEmail);
        public Task<UserDto> GetUserByEmail(string userEmail);
    }
}
