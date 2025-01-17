using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSRegister.CommonUtility.Models.Enums;
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
            var statusOrder = new List<int> { 2, 4, 6, 3, 5 };

            return await context.ProviderProfile
                .Include(p => p.Services)
                .Include(x => x.CabUser).ThenInclude(x => x.Cab)
                .OrderBy(c => statusOrder.IndexOf((int)c.ProviderStatus)) 
                .ThenBy(c => c.PublishedTime) 
                .Where(c=>c.ProviderStatus > ProviderStatusEnum.Unpublished)
                .ToListAsync();
        }


        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            return await context.ProviderProfile.Include(p => p.Services).Include(x => x.CabUser).ThenInclude(x => x.Cab)
            .Where(p=>p.Id == providerId && (p.ProviderStatus > ProviderStatusEnum.Unpublished)).FirstOrDefaultAsync() ?? new ProviderProfile();
            
        }
        public async Task<ProviderProfile> GetProviderWithServiceDetails(int providerId)
        {
             return await context.ProviderProfile
            .Include(p=>p.Services).ThenInclude(s=>s.ServiceRoleMapping).ThenInclude(s=>s.Role)
            .Include(p => p.Services).ThenInclude(s => s.ServiceQualityLevelMapping).ThenInclude(s => s.QualityLevel)
            .Include(p => p.Services).ThenInclude(s => s.ServiceIdentityProfileMapping).ThenInclude(s => s.IdentityProfile)
            .Include(p => p.Services).ThenInclude(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SupplementaryScheme)
            .Where(p => p.Id == providerId &&( p.ProviderStatus == ProviderStatusEnum.Published ||
            p.ProviderStatus == ProviderStatusEnum.PublishedActionRequired || p.ProviderStatus == ProviderStatusEnum.ActionRequired)).FirstOrDefaultAsync() ?? new ProviderProfile();
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
                        if(serviceStatus == ServiceStatusEnum.Published)
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

        public async Task<GenericResponse> UpdateRemovalStatus(EventTypeEnum eventType, TeamEnum team, int providerProfileId,
            List<int> serviceIds, string loggedInUserEmail, RemovalReasonsEnum? reason, ServiceRemovalReasonEnum? serviceRemovalReason)
        {
            GenericResponse genericResponse = new();
            ServiceStatusEnum serviceStatus = ServiceStatusEnum.AwaitingRemovalConfirmation;           
            DateTime?removedTime = null;
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existingProvider = await context.ProviderProfile.FirstOrDefaultAsync(p => p.Id == providerProfileId);
                if (existingProvider != null)
                {
                    if (eventType == EventTypeEnum.RemoveProvider) // remove provider and all published services under it
                    {
                        existingProvider.RemovalReason = reason;
                        existingProvider.ModifiedTime = DateTime.UtcNow;
                        existingProvider.RemovalRequestTime = DateTime.UtcNow;
                        existingProvider.ProviderStatus = ProviderStatusEnum.AwaitingRemovalConfirmation;

                        var existingServices = await context.Service.Where(e => serviceIds.Contains(e.Id)
                        && e.ProviderProfileId == providerProfileId && e.ServiceStatus == ServiceStatusEnum.Published).ToListAsync();
                        foreach (var existingService in existingServices)
                        {
                            existingService.ServiceStatus = serviceStatus;
                            existingService.ModifiedTime = DateTime.UtcNow;
                            existingService.RemovalRequestTime = DateTime.UtcNow;
                        }
                    }
                    else if (eventType == EventTypeEnum.RemoveService || eventType == EventTypeEnum.RemoveServiceRequestedByCab || eventType == EventTypeEnum.RemovedByCronJob)
                    {
                        if (eventType == EventTypeEnum.RemoveServiceRequestedByCab || eventType == EventTypeEnum.RemovedByCronJob)
                        {
                            serviceStatus = ServiceStatusEnum.Removed;
                            removedTime = DateTime.UtcNow;
                        }

                        foreach (var item in serviceIds)
                        {
                            var service = await context.Service.Where(s => s.Id == item && s.ProviderProfileId == providerProfileId).FirstOrDefaultAsync();
                            service.ServiceStatus = serviceStatus;
                            service.ModifiedTime = DateTime.UtcNow;
                            service.RemovalRequestTime = DateTime.UtcNow;
                            service.ServiceRemovalReason = serviceRemovalReason;
                            service.RemovedTime = removedTime;
                        }

                        if (existingProvider.Services.All(service => service.ServiceStatus == ServiceStatusEnum.Removed)) //to do : check different scenarios
                        {
                            existingProvider.RemovalReason = reason;
                            existingProvider.ModifiedTime = DateTime.UtcNow;
                            existingProvider.ProviderStatus = ProviderStatusEnum.RemovedFromRegister;
                            existingProvider.RemovedTime = DateTime.UtcNow;
                        }
                    }
                }

                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.RegisterManagement, loggedInUserEmail);
                await transaction.CommitAsync();
                genericResponse.Success = true;
            }
            catch (Exception ex)
            {
                genericResponse.Success = false;
                await transaction.RollbackAsync();
                logger.LogError(ex.Message);
            }
            return genericResponse;
        }


        public async Task<GenericResponse> SaveRemoveProviderToken(RemoveProviderToken removeProviderToken, TeamEnum team, EventTypeEnum eventType, string loggedinUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.RemoveProviderToken.FirstOrDefaultAsync(e => e.Token == removeProviderToken.Token && e.TokenId == removeProviderToken.TokenId);

                if (existingEntity == null)
                {
                    await context.RemoveProviderToken.AddAsync(removeProviderToken);
                    await context.SaveChangesAsync(team, eventType, loggedinUserEmail);
                    transaction.Commit();
                    genericResponse.Success = true;
                }

            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();
                logger.LogError($"Failed SaveRemoveProviderToken: {ex}");
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
                    if(providerStatus == ProviderStatusEnum.Published)
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
    }
}
