﻿using DVSAdmin.CommonUtility.Models;
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
        ProviderStatusEnum.UpdatesRequested,
        ProviderStatusEnum.AwaitingRemovalConfirmation,
        ProviderStatusEnum.Published,
        ProviderStatusEnum.RemovedFromRegister
            };

            return await context.ProviderProfile
                .Include(p => p.Services)
                .Include(x => x.CabUser).ThenInclude(x => x.Cab)
                .OrderBy(c => priorityOrder.IndexOf(c.ProviderStatus))
                .ThenByDescending(c => c.ModifiedTime)
                .Where(c => c.ProviderStatus > ProviderStatusEnum.Unpublished)
                .ToListAsync();
        }


        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            var priorityOrder = new List<ServiceStatusEnum>
            {
                ServiceStatusEnum.CabAwaitingRemovalConfirmation,
                ServiceStatusEnum.ReadyToPublish,
                ServiceStatusEnum.UpdatesRequested,
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

        public async Task<List<Service>> GetServiceVersionList(int serviceKey)
        {
            return await context.Service
            .Include(s => s.ServiceDraft)
            .Include(s => s.Provider).ThenInclude(p => p.ProviderProfileDraft)
            .Include(s => s.CertificateReview)
            .Include(s => s.ServiceSupSchemeMapping)
            .ThenInclude(s => s.SupplementaryScheme)
            .Include(s => s.ServiceRoleMapping)
            .ThenInclude(s => s.Role)
            .Include(s => s.ServiceQualityLevelMapping)
            .ThenInclude(s => s.QualityLevel)
            .Include(s => s.ServiceIdentityProfileMapping)
            .ThenInclude(s => s.IdentityProfile)
            .Where(s => s.ServiceKey == serviceKey)
            .ToListAsync();
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

        public async Task<GenericResponse> UpdateServiceStatus(List<int> serviceIds, int providerId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                foreach (var serviceId in serviceIds)
                {
                    var existingService = await context.Service.Include(s => s.Provider).FirstOrDefaultAsync(e => e.Id == serviceId);

                    var previousPublishedServiceVersionList = await context.Service.Where(s => s.ServiceKey == existingService.ServiceKey
                    && s.ServiceStatus == ServiceStatusEnum.Published && s.IsCurrent == false).ToListAsync();

                    if (existingService != null)
                    {

                        existingService.ServiceStatus = ServiceStatusEnum.Published;
                        existingService.IsInRegister = true;
                        existingService.Provider.IsInRegister = true;
                        existingService.ModifiedTime = DateTime.UtcNow;
                        existingService.PublishedTime = DateTime.UtcNow;


                        if (previousPublishedServiceVersionList != null && previousPublishedServiceVersionList.Count >0)
                        {
                            foreach (var version in previousPublishedServiceVersionList)
                            {
                                version.ServiceStatus = ServiceStatusEnum.Removed; // remove old published versions if any
                                version.RemovedTime = DateTime.UtcNow;
                            }
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

        public async Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog, string loggedInUserEmail, List<int> serviceIds)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {

                var existingProviderEntry = await context.RegisterPublishLog.FirstOrDefaultAsync(e => e.ProviderProfileId == registerPublishLog.ProviderProfileId);
                if (existingProviderEntry == null) {
                    registerPublishLog.Description = "First published";
                }
                else
                {          
                    
                    registerPublishLog.Description = serviceIds.Count + " new services included";
                }                

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