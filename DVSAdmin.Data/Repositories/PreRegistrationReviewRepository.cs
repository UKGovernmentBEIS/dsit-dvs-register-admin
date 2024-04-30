using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVSAdmin.Data.Repositories
{
    public class PreRegistrationReviewRepository : IPreRegistrationReviewRepository
    {

        private readonly DVSAdminDbContext context;
        private readonly ILogger<PreRegistrationReviewRepository> logger;

        public PreRegistrationReviewRepository(DVSAdminDbContext context, ILogger<PreRegistrationReviewRepository> logger)
        {
            this.context = context;
            this.logger=logger;
        }
        public async Task<PreRegistration> GetPreRegistration(int preRegistrationId)
        {
            PreRegistration preRegistration = new PreRegistration();
            preRegistration = await context.PreRegistration.Include(p => p.PreRegistrationReview).Include(p => p.UniqueReferenceNumber)
            .Include(p => p.PreRegistrationCountryMappings)
            .Where(p => p.Id == preRegistrationId).FirstOrDefaultAsync()?? new PreRegistration();
            return preRegistration;
        }


        public async Task<List<PreRegistration>> GetPreRegistrations()
        {
            return await context.PreRegistration.Include(p => p.PreRegistrationReview).Include(p => p.UniqueReferenceNumber).OrderBy(c => c.CreatedDate).ToListAsync();
        }

        public async Task<List<Country>> GetCountries()
        {
            return await context.Country.OrderBy(c => c.CountryName).ToListAsync();
        }


        public async Task<GenericResponse> SavePreRegistrationReview(PreRegistrationReview preRegistrationReview, ReviewTypeEnum reviewType)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.PreRegistrationReview.FirstOrDefaultAsync(e => e.PreRegistrationId == preRegistrationReview.PreRegistrationId);

                if (existingEntity != null)
                {
                    if (reviewType == ReviewTypeEnum.PrimaryCheck)
                    {
                        //update for existing draft record or for a secondary check
                        existingEntity.IsCountryApproved = preRegistrationReview.IsCountryApproved;
                        existingEntity.IsCompanyApproved= preRegistrationReview.IsCompanyApproved;
                        existingEntity.IsCheckListApproved= preRegistrationReview.IsCheckListApproved;
                        existingEntity.IsDirectorshipsApproved= preRegistrationReview.IsDirectorshipsApproved;
                        existingEntity.IsDirectorshipsAndRelationApproved= preRegistrationReview.IsDirectorshipsAndRelationApproved;
                        existingEntity.IsDirectorshipsAndRelationApproved= preRegistrationReview.IsDirectorshipsAndRelationApproved;
                        existingEntity.IsTradingAddressApproved= preRegistrationReview.IsTradingAddressApproved;
                        existingEntity.IsSanctionListApproved= preRegistrationReview.IsSanctionListApproved;
                        existingEntity.IsUNFCApproved= preRegistrationReview.IsUNFCApproved;
                        existingEntity.IsECCheckApproved= preRegistrationReview.IsECCheckApproved;
                        existingEntity.IsTARICApproved= preRegistrationReview.IsTARICApproved;
                        existingEntity.IsBannedPoliticalApproved= preRegistrationReview.IsBannedPoliticalApproved;
                        existingEntity.IsProvidersWebpageApproved= preRegistrationReview.IsProvidersWebpageApproved;
                        existingEntity.Comment = preRegistrationReview.Comment;
                        existingEntity.ApplicationReviewStatus = preRegistrationReview.ApplicationReviewStatus;
                        existingEntity.PrimaryCheckUserId = preRegistrationReview.PrimaryCheckUserId;
                        existingEntity.PrimaryCheckTime = DateTime.UtcNow;
                    }
                    else
                    {
                        existingEntity.ApplicationReviewStatus = preRegistrationReview.ApplicationReviewStatus;
                        existingEntity.Comment = preRegistrationReview.Comment;
                        existingEntity.RejectionReason = preRegistrationReview.RejectionReason;
                        existingEntity.SecondaryCheckUserId = preRegistrationReview.SecondaryCheckUserId;
                        existingEntity.SecondaryCheckTime = DateTime.UtcNow;
                    }

                    await context.SaveChangesAsync();

                }
                else
                {
                    preRegistrationReview.PrimaryCheckTime = DateTime.UtcNow;
                    await context.PreRegistrationReview.AddAsync(preRegistrationReview);
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



        public async Task<GenericResponse> UpdateURNStatus(UniqueReferenceNumber uniqueReferenceNumber)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.UniqueReferenceNumber.FirstOrDefaultAsync(e => e.PreRegistrationId == uniqueReferenceNumber.PreRegistrationId);

                if (existingEntity != null)
                {
                    existingEntity.URNStatus = uniqueReferenceNumber.URNStatus;
                    existingEntity.ModifiedDate= DateTime.UtcNow;
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

