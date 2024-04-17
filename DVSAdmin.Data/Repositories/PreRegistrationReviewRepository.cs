using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories
{
    public class PreRegistrationReviewRepository : IPreRegistrationReviewRepository
    {

        private readonly DVSAdminDbContext context;
        private readonly ILogger<PreRegistrationReviewRepository> logger;

        public PreRegistrationReviewRepository(DVSAdminDbContext context, ILogger<PreRegistrationReviewRepository> logger)
        {
            this.context = context;
            this.logger=logger;
        }
        public async Task<List<PreRegistration>> GetPreRegistrations()
        {
            return await context.PreRegistration.OrderBy(c => c.CreatedDate).ToListAsync();
        }
    }
}
