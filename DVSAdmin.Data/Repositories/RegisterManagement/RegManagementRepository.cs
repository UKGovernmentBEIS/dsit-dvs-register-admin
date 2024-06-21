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
            this.logger=logger;
        }

        public async Task<List<Provider>> GetProviders()
        {
            return await context.Provider.OrderBy(c => c.CreatedTime).ToListAsync();
        }

        public async Task<Provider> GetProviderDetails(int providerId)
        {
            Provider provider = new Provider();
            provider = await context.Provider.Include(p => p.PreRegistration)
           .Include(p => p.CertificateInformation)            
           .Where(p => p.Id == providerId).FirstOrDefaultAsync()?? new Provider();
            return provider;
        }
       
    }
}
