using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper automapper;
        

        public UserService(IUserRepository userRepository, IMapper automapper )
        {
            this.userRepository = userRepository;
            this.automapper = automapper;          
        }
        public async Task<UserDto> GetUser(string email)
        {
            var user = await userRepository.GetUser(email);           
            UserDto userDto = automapper.Map<UserDto>(user);           
            return userDto;
        }
        public async Task<List<string>> GetUserEmailsExcludingLoggedIn(string loggedInUserEmail)
        {         
            var userEmails = await userRepository.GetUserEmailsExcludingLoggedIn(loggedInUserEmail)?? new List<string>();
            return userEmails;
        }

        public async Task UpdateUserProfile(string loggedInUserEmail, string profile)
        {
            await userRepository.UpdateUserProfile(loggedInUserEmail, profile);
        }

    }
}
