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
            return await context.Service.Include(s => s.Provider).ThenInclude(s=>s.Services)
                .Where(s => s.ConformityExpiryDate <= DateTime.UtcNow && s.ServiceStatus == ServiceStatusEnum.Published)
                .ToListAsync();
        }

        public async Task MarkAsRemoved(List<int>? serviceIds)
        {
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

                    if (service.Provider.Services != null && service.Provider.Services.All(s => s.ServiceStatus == ServiceStatusEnum.Removed))
                    {
                        var provider = await context.ProviderProfile.Where(p => p.Id == service.ProviderProfileId).FirstOrDefaultAsync();
                        provider.ProviderStatus = ProviderStatusEnum.RemovedFromRegister;
                        provider.RemovalReason = RemovalReasonsEnum.RemovedByCronJob;
                        provider.RemovedTime = DateTime.UtcNow;
                    }
                }

                await context.SaveChangesAsync();
                transaction.Commit();
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError($"BackgroundJobRepository: MarkAsRemoved method failed: {ex}");
            }
        }
    }
}