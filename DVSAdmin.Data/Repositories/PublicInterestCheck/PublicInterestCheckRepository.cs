﻿using DVSAdmin.CommonUtility.Models;
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
            .Include(s => s.ProceedPublishConsentToken)
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
        public async Task<List<Service>> GetServiceList(int providerId)
        {
            return await context.Service.Where(s => s.ProviderProfileId == providerId).ToListAsync();
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

       
}
}
