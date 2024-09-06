using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories.PublicInterestCheck
{
    public class PublicInterestCheckRepository : IPublicInterestCheckRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<PublicInterestCheckRepository> logger;

        public PublicInterestCheckRepository(DVSAdminDbContext context, ILogger<PublicInterestCheckRepository> logger)
        {
            this.context = context;
            this.logger=logger;
        }
        public async Task<List<Service>> GetPICheckList()
        {           
            List<Service> piCheckList = await context.Service.Include(p => p.Provider).Include(p => p.PublicInterestCheck).Where(x => x.ServiceStatus == ServiceStatusEnum.Received).ToListAsync();      
            return piCheckList;
        }
    }
}
