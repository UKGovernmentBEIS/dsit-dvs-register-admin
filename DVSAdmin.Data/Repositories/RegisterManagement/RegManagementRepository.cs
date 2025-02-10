using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace DVSAdmin.Data.Repositories.RegisterManagement
{
    public class RegManagementRepository : IRegManagementRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<RegManagementRepository> logger;

        public RegManagementRepository(DVSAdminDbContext context, ILogger<RegManagementRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<List<ProviderProfile>> GetProviders()
        {
            var priorityOrder = new List<ProviderStatusEnum>
            {
        ProviderStatusEnum.CabAwaitingRemovalConfirmation,
        ProviderStatusEnum.ReadyToPublishNext,
        ProviderStatusEnum.ReadyToPublish,
        ProviderStatusEnum.AwaitingRemovalConfirmation,
        ProviderStatusEnum.Published,
        ProviderStatusEnum.RemovedFromRegister
            };

            return await context.ProviderProfile
                .Include(p => p.Services)
                .Include(x => x.CabUser).ThenInclude(x => x.Cab)
                .OrderBy(c => priorityOrder.IndexOf(c.ProviderStatus))
                .ThenBy(c => c.PublishedTime)
                .Where(c => c.ProviderStatus > ProviderStatusEnum.Unpublished)
                .ToListAsync();
        }


        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            var priorityOrder = new List<ServiceStatusEnum>
            {
                ServiceStatusEnum.CabAwaitingRemovalConfirmation,
                ServiceStatusEnum.ReadyToPublish,
                ServiceStatusEnum.Received,
                ServiceStatusEnum.AwaitingRemovalConfirmation,
                ServiceStatusEnum.Submitted,
                ServiceStatusEnum.Published,
                ServiceStatusEnum.Removed
             };

            return await context.ProviderProfile
                .Include(p => p.Services.OrderBy(s => priorityOrder.IndexOf(s.ServiceStatus))) 
                .Include(x => x.CabUser)
                .ThenInclude(x => x.Cab)
                .Include(p => p.Services).ThenInclude(x => x.CertificateReview)
                .Include(p => p.Services).ThenInclude(x => x.PublicInterestCheck)
                .Where(p => p.Id == providerId && (p.ProviderStatus > ProviderStatusEnum.Unpublished))
                .FirstOrDefaultAsync() ?? new ProviderProfile();
        }


        public async Task<Service> GetServiceDetails(int serviceId)
        {
            return await context.Service.Where(s => s.Id == serviceId).FirstOrDefaultAsync() ?? new Service(); ;

        }
        public async Task<ProviderProfile> GetProviderWithServiceDetails(int providerId)
        {
            return await context.ProviderProfile
           .Include(p => p.Services).ThenInclude(s => s.ServiceRoleMapping).ThenInclude(s => s.Role)
           .Include(p => p.Services).ThenInclude(s => s.ServiceQualityLevelMapping).ThenInclude(s => s.QualityLevel)
           .Include(p => p.Services).ThenInclude(s => s.ServiceIdentityProfileMapping).ThenInclude(s => s.IdentityProfile)
           .Include(p => p.Services).ThenInclude(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SupplementaryScheme)
           .Where(p => p.Id == providerId && (p.ProviderStatus == ProviderStatusEnum.Published ||
           p.ProviderStatus == ProviderStatusEnum.ReadyToPublishNext || p.ProviderStatus == ProviderStatusEnum.ReadyToPublish)).FirstOrDefaultAsync() ?? new ProviderProfile();
        }

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, ServiceStatusEnum serviceStatus, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                foreach (var serviceId in serviceIds)
                {
                    var existingService = await context.Service.FirstOrDefaultAsync(e => e.Id == serviceId);

                    if (existingService != null)
                    {
                        existingService.ServiceStatus = serviceStatus;
                        existingService.ModifiedTime = DateTime.UtcNow;
                        if (serviceStatus == ServiceStatusEnum.Published)
                        {
                            existingService.PublishedTime = DateTime.UtcNow;
                        }

                    }
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.RegisterManagement, loggedInUserEmail);
                }
                transaction.Commit();
                genericResponse.Success = true;
            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();
                logger.LogError(ex.Message);
            }
            return genericResponse;
        }

      
        public async Task<GenericResponse> UpdateProviderStatus(int providerId, ProviderStatusEnum providerStatus, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {

                var existingProvider = await context.ProviderProfile.FirstOrDefaultAsync(e => e.Id == providerId);
                if (existingProvider != null)
                {
                    existingProvider.ProviderStatus = providerStatus;
                    if (providerStatus == ProviderStatusEnum.Published)
                    {
                        existingProvider.PublishedTime = DateTime.UtcNow;
                    }
                }

                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.RegisterManagement, loggedInUserEmail);
                transaction.Commit();
                genericResponse.Success = true;
            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();
                logger.LogError(ex.Message);
            }
            return genericResponse;
        }

        public async Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                await context.RegisterPublishLog.AddAsync(registerPublishLog);
                await context.SaveChangesAsync();
                transaction.Commit();
                genericResponse.Success = true;
            }
            catch (Exception ex)
            {
                genericResponse.Success = false;
                transaction.Rollback();
                logger.LogError(ex.Message);
            }
            return genericResponse;
        }


        public async Task<List<Service>> GetPublishedServices()
        {
           return await context.Service.AsNoTracking()//Read only, so no need for tracking query
            .Include(service => service.Provider)
            .Include(service => service.CabUser)
                .ThenInclude(cabUser => cabUser.Cab)
            .Include(service => service.ServiceSupSchemeMapping)
                .ThenInclude(ssm => ssm.SupplementaryScheme)
            .Where(ci => ci.ServiceStatus >= ServiceStatusEnum.Published
                         && ci.ServiceStatus != ServiceStatusEnum.Removed 
                         && ci.ServiceStatus != ServiceStatusEnum.SavedAsDraft
                         && ci.Provider.ProviderStatus !=ProviderStatusEnum.RemovedFromRegister)
            .ToListAsync();
        }
    }
}