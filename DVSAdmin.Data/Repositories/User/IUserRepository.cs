using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.Data.Repositories
{
	public interface IUserRepository
	{
		public Task<GenericResponse> AddUser(User user);
		public Task<User> GetUser(string email);
		public Task<List<string>> GetUserEmailsExcludingLoggedIn(string loggedInUser);

    }
}

