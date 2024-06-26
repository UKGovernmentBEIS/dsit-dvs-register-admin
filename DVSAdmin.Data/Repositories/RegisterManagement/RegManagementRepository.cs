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
           .Include(p => p.CertificateInformation.OrderBy(c=>c.CreatedDate))            
           .Where(p => p.Id == providerId).OrderBy(c => c.CreatedTime).FirstOrDefaultAsync()?? new Provider();
            return provider;
        }

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds,int providerId, string userEmail, CertificateInfoStatusEnum certificateInfoStatus)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                foreach (var serviceId in serviceIds) 
                {
                    var existingService = await context.CertificateInformation.FirstOrDefaultAsync(e => e.Id == serviceId);

                    if (existingService != null)
                    {
                        existingService.CertificateInfoStatus = certificateInfoStatus;
                        existingService.ModifiedBy = userEmail;
                        existingService.ModifiedDate= DateTime.UtcNow;
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

                var existingProvider = await context.Provider.FirstOrDefaultAsync(e => e.Id == providerId);
                if (existingProvider!=null)
                {
                    existingProvider.ProviderStatus = providerStatus;
                    existingProvider.PublishedTime = DateTime.UtcNow;
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
    }
}
