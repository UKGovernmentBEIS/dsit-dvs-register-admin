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
                    }

                    await context.SaveChangesAsync();

                }
                else
                {
                    publicInterestCheck.PrimaryCheckTime = DateTime.UtcNow;
                    await context.PublicInterestCheck.AddAsync(publicInterestCheck);
                    await context.SaveChangesAsync();
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
    }
}
