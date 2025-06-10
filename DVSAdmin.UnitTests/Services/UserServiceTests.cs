using AutoMapper;
using DVSAdmin.BusinessLogic;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Repositories;
using NSubstitute;

namespace DVSAdmin.UnitTests.Services
{
    public class UserServiceTests
    {

        private readonly IUserRepository userRepository;
        private readonly IMapper automapper;        
        private readonly UserService userService;

        public UserServiceTests()
        {
            this.userRepository = Substitute.For<IUserRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            this.automapper = config.CreateMapper();            
            this.userService = new UserService(this.userRepository,this.automapper);
        }

        [Fact]
        public async Task GetUser_ShouldReturnUserDto()
        {
         
            var email = "test@example.com";
            var user = new User { Email = email }; 
            var userDto = new UserDto { Email = email };
            userRepository.GetUser(email).Returns(Task.FromResult(user));
            var result = await userService.GetUser(email);
            Assert.Equal(userDto.Email, result.Email);
            await userRepository.Received().GetUser(email);
    
        }

        [Fact]
        public async Task GetUserEmailsExcludingLoggedIn_ShouldReturnUserEmails()
        {
            
            var loggedInUserEmail = "loggedin@example.com";
            var userEmails = new List<string> { "user1@example.com", "user2@example.com" };
            userRepository.GetUserEmailsExcludingLoggedIn(loggedInUserEmail).Returns(Task.FromResult(userEmails));        
            var result = await userService.GetUserEmailsExcludingLoggedIn(loggedInUserEmail);       
            Assert.Equal(userEmails, result);
            await userRepository.Received().GetUserEmailsExcludingLoggedIn(loggedInUserEmail);
        }

        [Fact]
        public async Task GetUserEmailsExcludingLoggedIn_ShouldReturnEmptyListWhenNull()
        {
         
            var loggedInUserEmail = "loggedin@example.com";
           userRepository.GetUserEmailsExcludingLoggedIn(loggedInUserEmail).Returns(Task.FromResult<List<string>>(null!));
            var result = await userService.GetUserEmailsExcludingLoggedIn(loggedInUserEmail);
            Assert.Empty(result);
            await userRepository.Received().GetUserEmailsExcludingLoggedIn(loggedInUserEmail);
        }


        [Fact]
        public async Task UpdateUserProfile_ShouldCallUserRepositoryUpdateUserProfile()
        {       
            var loggedInUserEmail = "test@example.com";
            var profile = "Test";
            userRepository.UpdateUserProfile(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);
            await userService.UpdateUserProfile(loggedInUserEmail, profile);
            await userRepository.Received().UpdateUserProfile(loggedInUserEmail, profile);
        }

    }
}
