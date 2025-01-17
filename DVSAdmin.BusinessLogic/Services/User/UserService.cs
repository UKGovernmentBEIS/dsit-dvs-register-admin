using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Repositories;
using System.Diagnostics;

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
            // Log the loggedInUser to ensure it's not null or empty
            if (string.IsNullOrEmpty(loggedInUserEmail))
            {
                Debug.WriteLine("Logged-in user email is null or empty.");
                return new List<string>(); // Return an empty list if loggedInUser is null or empty
            }

            Debug.WriteLine($"User Email is {loggedInUserEmail}");
            var userEmails = await userRepository.GetUserEmailsExcludingLoggedIn(loggedInUserEmail);
            return userEmails;
        }

    }
}
