using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DVSAdmin.CommonUtility.Models.Enums;

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
            // only check the services which are already published to register. Status of services which are in review,but have an expired certificate will not be changed
            return await context.Service.Include(s => s.Provider).ThenInclude(s=>s.Services)
                .Where(s => s.ConformityExpiryDate < DateTime.Today && s.IsInRegister == true)
                .ToListAsync();
        }

        public async Task<bool> MarkAsRemoved(List<int>? serviceIds)
        {
            bool success = false;
            var servicesToRemove = await context.Service
                .Where(s => serviceIds != null && serviceIds.Contains(s.Id))
                .ToListAsync();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                foreach (var service in servicesToRemove)
                {
                    service.ServiceStatus = ServiceStatusEnum.Removed;
                    service.ModifiedTime = DateTime.UtcNow;
                    service.RemovedTime = DateTime.UtcNow;
                    service.ServiceRemovalReason = ServiceRemovalReasonEnum.RemovedByCronJob;
                    service.IsInRegister = false;
                }

                await context.SaveChangesAsync();
                transaction.Commit();
                success = true;
            }

            catch (Exception ex)
            {
                success = false;
                await transaction.RollbackAsync();
                logger.LogError($"BackgroundJobRepository: MarkAsRemoved method failed: {ex}");
            }

            return success;
        }
    }
}