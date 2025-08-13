using DVSAdmin.CommonUtility;
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
            .Include(p => p.PublicInterestCheck).Include(p => p.CabUser).ThenInclude(p => p.Cab)
             .ToListAsync();
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
            .Include(s => s.CabUser).ThenInclude(s => s.Cab).FirstOrDefaultAsync();


        }
        public async Task<Service> GetServiceDetailsWithMappings(int serviceId)
        {

            var baseQuery = context.Service
            .Where(p => p.Id == serviceId)
            .Include(p => p.Provider)
            .Include(p => p.PublicInterestCheck)
             .Include(p => p.TrustFrameworkVersion)
            .Include(p => p.UnderPinningService).ThenInclude(p=>p.Provider)
             .Include(p => p.UnderPinningService).ThenInclude(p => p.CabUser).ThenInclude(p=>p.Cab)
            .Include(p => p.ManualUnderPinningService).ThenInclude(x => x.Cab)
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

                queryWithOptionalIncludes = queryWithOptionalIncludes.Include(p => p.ServiceSupSchemeMapping)
               .ThenInclude(ssm => ssm.SchemeGPG44Mapping).ThenInclude(ssm => ssm.QualityLevel);

                queryWithOptionalIncludes = queryWithOptionalIncludes.Include(p => p.ServiceSupSchemeMapping)
                    .ThenInclude(ssm => ssm.SchemeGPG45Mapping).ThenInclude(ssm => ssm.IdentityProfile);
            }
            if (await baseQuery.AnyAsync(p => p.ServiceIdentityProfileMapping != null && p.ServiceIdentityProfileMapping.Any()))
            {
                queryWithOptionalIncludes = queryWithOptionalIncludes.Include(p => p.ServiceIdentityProfileMapping)
                    .ThenInclude(ssm => ssm.IdentityProfile);
            }
            var service = await queryWithOptionalIncludes.FirstOrDefaultAsync() ?? new Service();


            return service;
        }
        public async Task<List<Service>> GetServiceList(int providerId)
        {
            return await context.Service.Where(s => s.ProviderProfileId == providerId).ToListAsync();
        }

        public async Task<Service> GetServiceDetailsForPublishing(int serviceId)
        {
            return await context.Service
            .Include(s => s.Provider)
            .Include(s => s.TrustFrameworkVersion)
            .Include(s => s.UnderPinningService).ThenInclude(s=>s.Provider)
            .Include(s => s.UnderPinningService).ThenInclude(s => s.CabUser).ThenInclude(s=>s.Cab)
            .Include(s => s.ManualUnderPinningService)
            .Include(s => s.CabUser).ThenInclude(s => s.Cab)
            .Include(s => s.PublicInterestCheck)           
            .Include(s => s.ServiceRoleMapping).ThenInclude(s=>s.Role)
            .Include(s => s.ServiceIdentityProfileMapping).ThenInclude(s => s.IdentityProfile)
            .Include(s => s.ServiceQualityLevelMapping).ThenInclude(s => s.QualityLevel)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s=>s.SupplementaryScheme)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s=>s.SchemeGPG44Mapping).ThenInclude(s=>s.QualityLevel)
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SchemeGPG45Mapping).ThenInclude(s => s.IdentityProfile)
            .Where(s => s.Id == serviceId && s.ServiceStatus == ServiceStatusEnum.Received).FirstOrDefaultAsync();


        }
        public async Task<ProviderProfile> GetProviderDetailsWithOutReviewDetails(int providerId)
        {
            var priorityOrder = Helper.priorityOrderService;

            return await context.ProviderProfile
                .Include(p => p.Services.OrderBy(s => priorityOrder.IndexOf(s.ServiceStatus)))
                .Include(p => p.Services).ThenInclude(x => x.CabUser).ThenInclude(x => x.Cab)
                .Where(p => p.Id == providerId && (p.ProviderStatus > ProviderStatusEnum.Unpublished)).AsNoTracking()
                .FirstOrDefaultAsync() ?? new ProviderProfile();
        }
        public async Task<GenericResponse> SavePublicInterestCheck(PublicInterestCheck publicInterestCheck, ReviewTypeEnum reviewType, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existingEntity = await context.PublicInterestCheck.Include(e => e.Service).FirstOrDefaultAsync(e => e.ServiceId == publicInterestCheck.ServiceId
                    && e.ProviderProfileId == publicInterestCheck.ProviderProfileId);

                if (existingEntity != null)
                {
                    if (reviewType == ReviewTypeEnum.PrimaryCheck)
                    {
                        existingEntity.IsCompanyHouseNumberApproved = publicInterestCheck.IsCompanyHouseNumberApproved;
                        existingEntity.IsDirectorshipsApproved = publicInterestCheck.IsDirectorshipsApproved;
                        existingEntity.IsDirectorshipsAndRelationApproved = publicInterestCheck.IsDirectorshipsAndRelationApproved;
                        existingEntity.IsTradingAddressApproved = publicInterestCheck.IsTradingAddressApproved;
                        existingEntity.IsSanctionListApproved = publicInterestCheck.IsSanctionListApproved;
                        existingEntity.IsUNFCApproved = publicInterestCheck.IsUNFCApproved;
                        existingEntity.IsECCheckApproved = publicInterestCheck.IsECCheckApproved;
                        existingEntity.IsTARICApproved = publicInterestCheck.IsTARICApproved;
                        existingEntity.IsBannedPoliticalApproved = publicInterestCheck.IsBannedPoliticalApproved;
                        existingEntity.IsProvidersWebpageApproved = publicInterestCheck.IsProvidersWebpageApproved;
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

                    if (existingEntity.Service != null)
                    {
                        existingEntity.Service.ModifiedTime = DateTime.UtcNow;
                    }

                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.PICheck, loggedInUserEmail);
                    genericResponse.InstanceId = existingEntity.Id;
                }
                else
                {
                    publicInterestCheck.PrimaryCheckTime = DateTime.UtcNow;
                    var service = await context.Service.FirstOrDefaultAsync(e => e.Id == publicInterestCheck.ServiceId);

                    if (service != null)
                    {
                        service.ModifiedTime = DateTime.UtcNow;
                    }
                    var entity = await context.PublicInterestCheck.AddAsync(publicInterestCheck);
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.PICheck, loggedInUserEmail);

                    genericResponse.InstanceId = entity.Entity.Id;
                }

                await transaction.CommitAsync();
                genericResponse.Success = true;
            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                await transaction.RollbackAsync();
                logger.LogError($"Error saving Public Interest Check: {ex.Message}", ex);
            }
            return genericResponse;
        }

        public async Task<GenericResponse> SavePICheckLog(PICheckLogs pICheck, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {               
                await context.PICheckLogs.AddAsync(pICheck);
                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.PICheck, loggedInUserEmail);
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

        public async Task<GenericResponse> UpdateServiceStatus(int serviceId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
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


                    if (previousPublishedServiceVersionList != null && previousPublishedServiceVersionList.Count > 0)
                    {
                        foreach (var version in previousPublishedServiceVersionList)
                        {
                            version.ServiceStatus = ServiceStatusEnum.Removed; // remove old published versions if any
                            version.RemovedTime = DateTime.UtcNow;
                            version.IsInRegister = false;
                        }
                    }

                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.PICheck, loggedInUserEmail);

                    if (await AddTrustMarkNumber(existingService.Id, existingService.ServiceKey, existingService.Provider.Id, loggedInUserEmail))
                    {
                        await transaction.CommitAsync();
                        genericResponse.Success = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        genericResponse.Success = false;
                    }

                }
                else
                {
                    transaction.Rollback();
                    genericResponse.Success = false;
                    Console.WriteLine("Service details fetched as null");
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

        public async Task<GenericResponse> SavePublishRegisterLog(RegisterPublishLog registerPublishLog, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {

                var existingProviderEntry = await context.RegisterPublishLog.FirstOrDefaultAsync(e => e.ProviderProfileId == registerPublishLog.ProviderProfileId);
                if (existingProviderEntry == null)
                {
                    registerPublishLog.Description = "First published";
                }
                else
                {

                    registerPublishLog.Description = "1 new service included";
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
        private async Task<bool> AddTrustMarkNumber(int serviceId, int serviceKey, int providerId, string loggedInUserEmail)
        {
            bool success = false;
            try
            {
                int serviceNumber;
                int companyId;
                var existingTrustmarkWithServiceKey = await context.TrustmarkNumber.FirstOrDefaultAsync(t => t.ServiceKey == serviceKey);
                if (existingTrustmarkWithServiceKey == null)
                {
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
                        companyId = maxCompanyId + 1;
                        serviceNumber = 0; // service number initialize to 0 if doesnt exist
                    }

                    TrustmarkNumber trustmarkNumber = new()
                    {
                        ProviderProfileId = providerId,
                        ServiceId = serviceId,
                        CompanyId = companyId,
                        ServiceNumber = serviceNumber + 1, // service id start with 1 
                        TimeStamp = DateTime.UtcNow,
                        ServiceKey = serviceKey

                    };

                    await context.TrustmarkNumber.AddAsync(trustmarkNumber);
                    await context.SaveChangesAsync(TeamEnum.Provider, EventTypeEnum.TrustmarkNumberGeneration, loggedInUserEmail);
                    success = true;
                }
                else
                {
                    success = true; // if already TM number with same service key exists , donot generate new TM number
                    Console.WriteLine("TM number already generated for first version:ProviderId{0} ServiceId: {1} ServiceKey: {2}", providerId, serviceId, serviceKey);
                }

            }
            catch (Exception ex)
            {
                success = false;
                logger.LogError($"Failed to generate trustmark number: {ex}");
                Console.WriteLine("ProviderId:{0} serviceId: {1}", providerId, serviceId);
            }
            return success;

        }
    }
}
