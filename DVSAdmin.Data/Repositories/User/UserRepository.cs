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
    }
}

