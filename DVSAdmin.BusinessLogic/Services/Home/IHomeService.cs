using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Entities;
namespace DVSAdmin.BusinessLogic.Services
{
    public interface IHomeService
    {
        public Task<PaginatedResult<ServiceDto>> GetServices(int pageNumber, string sort, string sortAction, string openTask);
        public Task<Dictionary<string, int>> GetPendingCounts();
        public Task<UserDto> GetUserByEmail(string userEmail);
    }
}
