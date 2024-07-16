using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IUserService
    {
        public Task<UserDto> GetUser(string email);
    }
}
