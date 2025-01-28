﻿using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace DVSAdmin.Data.Repositories.RemoveProvider
{
    public class RemoveProviderRepository : IRemoveProviderRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<RemoveProviderRepository> logger;

        public RemoveProviderRepository(DVSAdminDbContext context, ILogger<RemoveProviderRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            return await context.ProviderProfile.Include(p => p.Services).Include(x => x.CabUser).Include(x => x.CabUser).ThenInclude(x => x.Cab)
             .Include(p => p.Services).ThenInclude(x => x.CertificateReview).Include(p => p.Services).ThenInclude(x => x.PublicInterestCheck)
            .Where(p => p.Id == providerId && (p.ProviderStatus > ProviderStatusEnum.Unpublished)).FirstOrDefaultAsync() ?? new ProviderProfile();

        }

        public async Task<Service> GetServiceDetails(int serviceId)
        {
            return await context.Service.Include(s=>s.Provider).Include(s=>s.CabUser).Where(s => s.Id == serviceId).FirstOrDefaultAsync() ?? new Service(); ;

        }
        public async Task<GenericResponse> UpdateProviderStatus(int providerProfileId, ProviderStatusEnum providerStatus, string loggedInUserEmail, EventTypeEnum eventType)
        {
            GenericResponse genericResponse = new();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existingProvider = await context.ProviderProfile.FirstOrDefaultAsync(p => p.Id == providerProfileId);
                if (existingProvider != null)
                {

                    existingProvider.ModifiedTime = DateTime.UtcNow;
                    existingProvider.ProviderStatus = providerStatus;

                    if (providerStatus == ProviderStatusEnum.RemovedFromRegister)
                    {
                        existingProvider.RemovedTime = DateTime.UtcNow;
                    }
                    else if (providerStatus == ProviderStatusEnum.AwaitingRemovalConfirmation)
                    {
                        existingProvider.RemovalRequestTime = DateTime.UtcNow;
                    }
                }
                await context.SaveChangesAsync(TeamEnum.DSIT, eventType, loggedInUserEmail);
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

        public async Task<GenericResponse> RemoveServiceRequestByCab(int providerProfileId, List<int> serviceIds, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();


            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in serviceIds)
                {
                    var service = await context.Service.Where(s => s.Id == item && s.ProviderProfileId == providerProfileId).FirstOrDefaultAsync();
                    var currentStatus = service.ServiceStatus;

                    if (currentStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation)
                    {
                        service.ServiceStatus = ServiceStatusEnum.Removed;
                    }
                    else
                    {
                        service.ServiceStatus = ServiceStatusEnum.AwaitingRemovalConfirmation;
                    }
                    service.ModifiedTime = DateTime.UtcNow;
                    service.RemovalRequestTime = DateTime.UtcNow;                  
                }
                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.RemoveService, loggedInUserEmail);
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
    }
}

