using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories
{
    public class PublicInterestCheckRepository : IPublicInterestCheckRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<PublicInterestCheckRepository> logger;

        public PublicInterestCheckRepository(DVSAdminDbContext context, ILogger<PublicInterestCheckRepository> logger)
        {
            this.context = context;
            this.logger=logger;
        }
        public async Task<List<Service>> GetPICheckList()
        {
            List<Service> piCheckList = await context.Service.Include(p => p.Provider)
            .Include(p => p.PublicInterestCheck).Include(p => p.CabUser).ThenInclude(p => p.Cab).
             Where(x => x.ServiceStatus == ServiceStatusEnum.Received).ToListAsync();
            return piCheckList;
        }


        public async Task<Service> GetServiceDetails(int serviceId)
        {
            return await context.Service
            .Include(s => s.Provider)
            .Include(s => s.CabUser).ThenInclude(s => s.Cab)
            .Include(s => s.PublicInterestCheck)
            .Include(s => s.ServiceRoleMapping)
            .Include(s => s.PublicInterestCheck).ThenInclude(p => p.PrimaryCheckUser)
            .Include(s => s.PublicInterestCheck).ThenInclude(p => p.SecondaryCheckUser)
            .Where(s => s.Id == serviceId)
            .Include(s => s.CabUser).ThenInclude(s => s.Cab).FirstOrDefaultAsync()?? new();


        }



        public async Task<Service> GetServiceDetailsWithMappings(int serviceId)
        {

            var baseQuery = context.Service
            .Where(p => p.Id == serviceId)
            .Include(p => p.Provider)
            .Include(p => p.PublicInterestCheck)
            .Include(p => p.CabUser).ThenInclude(cu => cu.Cab)
            .Include(p => p.ServiceRoleMapping)
            .ThenInclude(s => s.Role);



            IQueryable<Service> queryWithOptionalIncludes = baseQuery;
            if (await baseQuery.AnyAsync(p => p.ServiceQualityLevelMapping != null && p.ServiceQualityLevelMapping.Any()))
            {
                queryWithOptionalIncludes = queryWithOptionalIncludes.Include(p => p.ServiceQualityLevelMapping)
                    .ThenInclude(sq => sq.QualityLevel);
            }

            if (await baseQuery.AnyAsync(p => p.ServiceSupSchemeMapping != null && p.ServiceSupSchemeMapping.Any()))
            {
                queryWithOptionalIncludes = queryWithOptionalIncludes.Include(p => p.ServiceSupSchemeMapping)
                    .ThenInclude(ssm => ssm.SupplementaryScheme);
            }
            if (await baseQuery.AnyAsync(p => p.ServiceIdentityProfileMapping != null && p.ServiceIdentityProfileMapping.Any()))
            {
                queryWithOptionalIncludes = queryWithOptionalIncludes.Include(p => p.ServiceIdentityProfileMapping)
                    .ThenInclude(ssm => ssm.IdentityProfile);
            }
            var service = await queryWithOptionalIncludes.FirstOrDefaultAsync() ?? new Service();


            return service;
        }

        public async Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheck publicInterestCheck, ReviewTypeEnum reviewType)
        {
            GenericResponse genericResponse = new ();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.PublicInterestCheck.FirstOrDefaultAsync(e => e.ServiceId == publicInterestCheck.ServiceId
                && e.ProviderProfileId == publicInterestCheck.ProviderProfileId);

                if (existingEntity != null)
                {
                    if (reviewType == ReviewTypeEnum.PrimaryCheck)
                    {
                        //update for existing draft record or for a secondary check                      
                        existingEntity.IsCompanyHouseNumberApproved= publicInterestCheck.IsCompanyHouseNumberApproved;
                        existingEntity.IsDirectorshipsApproved= publicInterestCheck.IsDirectorshipsApproved;
                        existingEntity.IsDirectorshipsAndRelationApproved= publicInterestCheck.IsDirectorshipsAndRelationApproved;
                        existingEntity.IsDirectorshipsAndRelationApproved= publicInterestCheck.IsDirectorshipsAndRelationApproved;
                        existingEntity.IsTradingAddressApproved= publicInterestCheck.IsTradingAddressApproved;
                        existingEntity.IsSanctionListApproved= publicInterestCheck.IsSanctionListApproved;
                        existingEntity.IsUNFCApproved= publicInterestCheck.IsUNFCApproved;
                        existingEntity.IsECCheckApproved= publicInterestCheck.IsECCheckApproved;
                        existingEntity.IsTARICApproved= publicInterestCheck.IsTARICApproved;
                        existingEntity.IsBannedPoliticalApproved= publicInterestCheck.IsBannedPoliticalApproved;
                        existingEntity.IsProvidersWebpageApproved= publicInterestCheck.IsProvidersWebpageApproved;
                        existingEntity.PrimaryCheckComment = publicInterestCheck.PrimaryCheckComment;
                        existingEntity.PublicInterestCheckStatus = publicInterestCheck.PublicInterestCheckStatus;
                        existingEntity.PrimaryCheckUserId = publicInterestCheck.PrimaryCheckUserId;
                        existingEntity.PrimaryCheckTime = DateTime.UtcNow;
                    }
                    else
                    {
                        existingEntity.PublicInterestCheckStatus = publicInterestCheck.PublicInterestCheckStatus;
                        existingEntity.SecondaryCheckComment = publicInterestCheck.SecondaryCheckComment;
                        existingEntity.RejectionReason = publicInterestCheck.RejectionReason;
                        existingEntity.SecondaryCheckUserId = publicInterestCheck.SecondaryCheckUserId;
                        existingEntity.SecondaryCheckTime = DateTime.UtcNow;
                        existingEntity.RejectionReasons = publicInterestCheck.RejectionReasons;
                    }

                    await context.SaveChangesAsync();
                    genericResponse.InstanceId = existingEntity.Id;

                }
                else
                {
                  
                    publicInterestCheck.PrimaryCheckTime = DateTime.UtcNow;
                    var entity = await context.PublicInterestCheck.AddAsync(publicInterestCheck);                 
                    await context.SaveChangesAsync();
                    genericResponse.InstanceId = entity.Entity.Id;
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

        public async Task<GenericResponse> SavePICheckLog(PICheckLogs pICheck)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {               
                await context.PICheckLogs.AddAsync(pICheck);
                await context.SaveChangesAsync();
                transaction.Commit();
                genericResponse.Success = true;
            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();
                logger.LogError($"Failed to log to PI check logs: {ex}");              

            }
            return genericResponse;
        }


        public async Task<List<Service>> GetServiceList(int providerId)
        {
            return await context.Service .Where(s => s.ProviderProfileId == providerId).ToListAsync();
        }


        public async Task<GenericResponse> UpdateServiceAndProviderStatus(int serviceId,  ProviderStatusEnum providerStatus)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {

                var serviceEntity = await context.Service.FirstOrDefaultAsync(e => e.Id == serviceId);
                var providerEntity = await context.ProviderProfile.FirstOrDefaultAsync(e => e.Id == serviceEntity.ProviderProfileId);

                if (serviceEntity != null && providerEntity != null)
                {                  
                    serviceEntity.ServiceStatus = ServiceStatusEnum.ReadyToPublish;
                    serviceEntity.ModifiedTime = DateTime.UtcNow;   
                    providerEntity.ProviderStatus = providerStatus;
                    providerEntity.ModifiedTime = DateTime.UtcNow;
                    await context.SaveChangesAsync();

                    if(await AddTrustMarkNumber(serviceEntity.Id,providerEntity.Id)) 
                    {
                        transaction.Commit();
                        genericResponse.Success = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        genericResponse.Success = false;
                    }                   
                }
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



      

        private async Task<bool> AddTrustMarkNumber(int serviceId, int providerId)
        {
            bool success = false;
            try
            {
                int serviceNumber;
                int companyId;
                var existingTrustmark = await context.TrustmarkNumber.FirstOrDefaultAsync(t => t.ProviderProfileId == providerId);
                if (existingTrustmark != null)
                {
                    // If it exists, select the existing CompanyId
                    // select max of service number, if doesnt exist set as 0                    
                    serviceNumber = await context.TrustmarkNumber.Where(p => p.ProviderProfileId == providerId).MaxAsync(p => (int?)p.ServiceNumber) ?? 0;
                    companyId = existingTrustmark.CompanyId;

                }
                else
                {
                    //If doesn't exist, get max company id or return initial value as 199 and then increment by 1
                    int maxCompanyId = await context.TrustmarkNumber.MaxAsync(t => (int?)t.CompanyId) ?? 199;
                    companyId = maxCompanyId+1;
                    serviceNumber = 0; // service number initialize to 0 if doesnt exist
                }

                TrustmarkNumber trustmarkNumber = new()
                {
                    ProviderProfileId = providerId,
                    ServiceId = serviceId,
                    CompanyId = companyId,
                    ServiceNumber = serviceNumber+1, // service id start with 1 
                    TimeStamp = DateTime.UtcNow

                };

                await context.TrustmarkNumber.AddAsync(trustmarkNumber);
                await context.SaveChangesAsync();
                success = true;
            }
            catch (Exception ex)
            {
                success =false;
                logger.LogError($"Failed to generate trustmark number: {ex}");
                logger.LogInformation("ProviderId:{0} serviceId: {1}", providerId, serviceId);
            }
           return success;
        
          }
}
}
