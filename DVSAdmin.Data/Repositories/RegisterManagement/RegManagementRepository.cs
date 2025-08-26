using DVSAdmin.CommonUtility;
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
            var priorityOrder = Helper.priorityOrderProvider;

            return await context.ProviderProfile
                .Include(p => p.Services)
                .Include(p => p.Services).ThenInclude(x => x.CabUser).ThenInclude(x => x.Cab)
                .OrderBy(c => priorityOrder.IndexOf(c.ProviderStatus))
                .ThenByDescending(c => c.ModifiedTime)
                .Where(c => c.ProviderStatus > ProviderStatusEnum.Unpublished)
                .ToListAsync();
        }

        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            var priorityOrder = Helper.priorityOrderService;

            return await context.ProviderProfile
                .Include(p => p.Services.OrderBy(s => priorityOrder.IndexOf(s.ServiceStatus)))
                .Include(p => p.Services).ThenInclude(x => x.CabUser).ThenInclude(x => x.Cab)
                .Include(p => p.Services).ThenInclude(x => x.CertificateReview)
                .Include(p => p.Services).ThenInclude(x => x.PublicInterestCheck)
                .Where(p => p.Id == providerId && (p.ProviderStatus > ProviderStatusEnum.Unpublished))
                .FirstOrDefaultAsync() ?? new ProviderProfile();
        }


      

        public async Task<List<Service>> GetServiceVersionList(int serviceKey)
        {
            return await context.Service
            .Include(s => s.ServiceDraft)
            .Include(s => s.Provider).ThenInclude(p => p.ProviderProfileDraft)
            .Include(s => s.CertificateReview)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SupplementaryScheme)
            .Include(s => s.ServiceRoleMapping).ThenInclude(s => s.Role)
            .Include(s => s.ServiceQualityLevelMapping).ThenInclude(s => s.QualityLevel)
            .Include(s => s.ServiceIdentityProfileMapping).ThenInclude(s => s.IdentityProfile)
            .Include(s => s.TrustFrameworkVersion)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SupplementaryScheme)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SchemeGPG44Mapping).ThenInclude(s => s.QualityLevel)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SchemeGPG45Mapping).ThenInclude(s => s.IdentityProfile)
            .Include(s => s.UnderPinningService).ThenInclude(s => s.Provider)
            .Include(s => s.UnderPinningService).ThenInclude(s => s.CabUser).ThenInclude(s=>s.Cab)
            .Include(s=>s.CabUser).ThenInclude(s=>s.Cab)
            .Include(s => s.ManualUnderPinningService).ThenInclude(s => s.Cab)
            .Where(s => s.ServiceKey == serviceKey).AsNoTracking()
            .ToListAsync();
        }

        public async Task<Service> GetServiceDetails(int serviceId)
        {
            return await context.Service.Where(s => s.Id == serviceId).FirstOrDefaultAsync() ?? new Service(); ;

        }
    

        public async Task<List<string>> GetCabEmailListForServices(List<int> serviceIds)
        {
            List<int> cabIds = await context.Service.Include(p => p.CabUser).Where(x => serviceIds.Contains(x.Id)).Select(x => x.CabUser.CabId).Distinct().ToListAsync();
            List<string> activeCabUserEmails = await context.CabUser.Where(c => cabIds.Contains(c.CabId) && c.IsActive).Select(c => c.CabEmail).ToListAsync();
            return activeCabUserEmails;
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