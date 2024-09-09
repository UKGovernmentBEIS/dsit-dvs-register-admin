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
            return await context.ProviderProfile
           .Include(p => p.Services.Where(x => x.ServiceStatus == ServiceStatusEnum.Published
            ||x.ServiceStatus  == ServiceStatusEnum.ReadyToPublish)).Include(x=>x.CabUser).ThenInclude(x=>x.Cab)
            .OrderBy(c => c.ModifiedTime).ToListAsync();
            
        }

        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            return await context.ProviderProfile.Include(p => p.Services).Include(x => x.CabUser).ThenInclude(x => x.Cab)
            .Where(p=>p.Id == providerId && (p.ProviderStatus == ProviderStatusEnum.Published ||
            p.ProviderStatus == ProviderStatusEnum.PublishedActionRequired || p.ProviderStatus == ProviderStatusEnum.ActionRequired)).FirstOrDefaultAsync() ?? new ProviderProfile();
            
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

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, ServiceStatusEnum serviceStatus)
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
                    context.SaveChanges();
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

        public async Task<GenericResponse> UpdateProviderStatus(int providerId, ProviderStatusEnum providerStatus)
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

                context.SaveChanges();
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

        public async Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog)
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
