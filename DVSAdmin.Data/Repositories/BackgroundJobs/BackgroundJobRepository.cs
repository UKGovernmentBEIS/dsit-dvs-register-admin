using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories.BackgroundJobs

{
    public class BackgroundJobRepository : IBackgroundJobRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<BackgroundJobRepository> logger;
        
        public BackgroundJobRepository(DVSAdminDbContext context, ILogger<BackgroundJobRepository> logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Service>> GetExpiredCertificates()
        { 
            return await context.Service
                .Where(s => s.ConformityExpiryDate <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task MarkAsRemoved(List<int>? serviceIds)
        {
            if (serviceIds == null || !serviceIds.Any())
            {
                logger.LogError("Service IDs cannot be null or empty: {$1}", nameof(serviceIds));
            }
            
            var servicesToRemove = await context.Service
                .Where(s => serviceIds != null && serviceIds.Contains(s.Id))
                .ToListAsync();

            if (!servicesToRemove.Any())
            {
                logger.LogError("No services found with the provided IDs");
            }
            
            foreach (var service in servicesToRemove)
            {
                service.ServiceStatus = ServiceStatusEnum.Removed;
            }
            await context.SaveChangesAsync();
        }
    }
}