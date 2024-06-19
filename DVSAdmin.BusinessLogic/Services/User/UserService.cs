using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper automapper;
        private readonly ILogger<PreRegistrationReviewService> logger;

        public UserService(IUserRepository userRepository, IMapper automapper,
          ILogger<PreRegistrationReviewService> logger)
        {
            this.userRepository = userRepository;
            this.automapper = automapper;
            this.logger = logger;
        }
        public async Task<UserDto> GetUser(string email)
        {
            var user = await userRepository.GetUser(email);           
            UserDto userDto = automapper.Map<UserDto>(user);           
            return userDto;
        }
    }
}
