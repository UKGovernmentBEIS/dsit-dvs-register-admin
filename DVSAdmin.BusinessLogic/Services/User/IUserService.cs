using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IUserService
    {
        public Task<UserDto> GetUser(string email);

        public Task<List<string>> GetUserEmailsExcludingLoggedIn(string loggedInUserEmail, string profile);
        public Task UpdateUserProfile(string loggedInUserEmail, string profile);
    }
}
