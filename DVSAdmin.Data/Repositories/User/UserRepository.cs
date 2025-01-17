using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DVSAdmin.Data.Repositories
{
    public class UserRepository : IUserRepository
	{
        private readonly DVSAdminDbContext context;

        public UserRepository(DVSAdminDbContext context)
        {
            this.context = context;
        }

        public async Task<GenericResponse> AddUser(User user)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.User.FirstOrDefaultAsync<User>(e => e.Email == user.Email);
                if(existingEntity == null)
                {
                    user.CreatedDate = DateTime.UtcNow.Date;
                    await context.User.AddAsync(user);
                }
                else
                {
                    existingEntity.ModifiedDate = DateTime.UtcNow.Date;
                    existingEntity.Email = user.Email;
                }
                
                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.Adduser, user.Email);
                transaction.Commit();
                genericResponse.Success = true;
            }
            catch(Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();

                Console.WriteLine($"Exception while adding user to table - {ex}");
            }

            return genericResponse;
        }

        public async Task<User> GetUser(string email)
        {
            User user = new User();
            user = await context.User.FirstOrDefaultAsync<User>(e => e.Email == email);
            return user;
        }

        public async Task<List<string>> GetUserEmailsExcludingLoggedIn(string loggedInUser, string profile)
        { 
            List<string> userEmails = await context.User.Where(u => u.Email != loggedInUser && u.Profile == profile) 
            .Select(u => u.Email).ToListAsync()??new List<string>();
            return userEmails;
        }

        public async Task UpdateUserProfile(string loggedInUserEmail, string profile)
        {

            using var transaction = context.Database.BeginTransaction();
            User user = new();
            try
            {
                var existingEntity = await context.User.FirstOrDefaultAsync(e => e.Email == loggedInUserEmail);
                if (existingEntity != null &&  string.IsNullOrEmpty(existingEntity.Profile))
                {
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    existingEntity.Profile = profile;
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.UpdateUser);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.Write($"Exception while updatding user profile - {ex}");
            }
        }
    }
}

