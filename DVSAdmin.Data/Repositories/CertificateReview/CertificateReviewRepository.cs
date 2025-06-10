using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DVSAdmin.Data.Repositories
{
    public class CertificateReviewRepository : ICertificateReviewRepository
    {
        private readonly DVSAdminDbContext context;
        private readonly ILogger<CertificateReviewRepository> logger;

        public CertificateReviewRepository(DVSAdminDbContext context, ILogger<CertificateReviewRepository> logger)
        {
            this.context = context;
            this.logger=logger;
        }
       

        

        public async Task<CertificateReview> GetCertificateReview(int reviewId)
        {
            CertificateReview certificateReview = new ();
            certificateReview = await context.CertificateReview
            .Where(p => p.Id == reviewId).FirstOrDefaultAsync()?? new CertificateReview();
            return certificateReview;
        }

        public async Task<CertificateReview> GetCertificateReviewWithRejectionData(int reviewId)
        {
            CertificateReview certificateReview = new ();
            certificateReview = await context.CertificateReview.Include(p => p.CertificateReviewRejectionReasonMapping)
            .Where(p => p.Id == reviewId).FirstOrDefaultAsync()?? new CertificateReview();
            return certificateReview;
        }

        public async Task<List<Role>> GetRoles()
        {
            return await context.Role.OrderBy(c => c.RoleName).ToListAsync();
        }

        public async Task<List<IdentityProfile>> GetIdentityProfiles()
        {
            return await context.IdentityProfile.OrderBy(c => c.IdentityProfileName).ToListAsync();
        }

        public async Task<List<SupplementaryScheme>> GetSupplementarySchemes()
        {
            return await context.SupplementaryScheme.OrderBy(c => c.SchemeName).ToListAsync();
        }

        public async Task<List<CertificateReviewRejectionReason>> GetRejectionReasons()
        {
            return await context.CertificateReviewRejectionReason.ToListAsync();
        }       

       
        public async Task<List<Service>> GetServiceList()
        {
            return await context.Service
            .Include(s => s.Provider)
            .Include(s => s.CertificateReview)
            .Include(s => s.ServiceRoleMapping)
            .Include(s => s.CabUser).ThenInclude(s => s.Cab)
            .OrderByDescending(s => s.CreatedTime)
            .ToListAsync();
        }

     



        public async Task<Service> GetServiceDetails(int serviceId)
        {

            var baseQuery = context.Service
            .Where(p => p.Id == serviceId)
            .Include(p => p.Provider)
            .Include(p => p.CertificateReview)
            .Include (p => p.ProceedApplicationConsentToken)
            .Include(p => p.CabUser).ThenInclude(cu => cu.Cab)
            .Include(p => p.ServiceRoleMapping)
            .ThenInclude(s => s.Role).AsSplitQuery(); 



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

        public async Task<List<string>> GetCabEmailListForProvider(int providerId)
        {
            return await context.Service.Include(p => p.CabUser).Where(x => x.ProviderProfileId == providerId).Select(x => x.CabUser.CabEmail).Distinct().ToListAsync();

        }

        public async Task<List<string>> GetCabEmailListForServices(List<int> serviceIds)
        {
            return await context.Service.Include(p => p.CabUser).Where(x =>  serviceIds.Contains(x.Id)).Select(x => x.CabUser.CabEmail).Distinct().ToListAsync();

        }

        #region Save, update

        //First save -  Part 1 of 2: Certificate Validation Continue and save as draft flow
        public async Task<GenericResponse> SaveCertificateReview(CertificateReview cetificateReview, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.Include(p => p.CertificateReviewRejectionReasonMapping).FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId && e.ProviProviderProfileId ==cetificateReview.ProviProviderProfileId);

                if (existingEntity != null)
                {
                    existingEntity.IsCabLogoCorrect = cetificateReview.IsCabLogoCorrect;
                    existingEntity.IsCabDetailsCorrect  = cetificateReview.IsCabDetailsCorrect;
                    existingEntity.IsProviderDetailsCorrect = cetificateReview.IsProviderDetailsCorrect;
                    existingEntity.IsServiceNameCorrect = cetificateReview.IsServiceNameCorrect;
                    existingEntity.IsRolesCertifiedCorrect = cetificateReview.IsRolesCertifiedCorrect;
                    existingEntity.IsCertificationScopeCorrect = cetificateReview.IsCertificationScopeCorrect;
                    existingEntity.IsServiceSummaryCorrect = cetificateReview.IsServiceSummaryCorrect;
                    existingEntity.IsURLLinkToServiceCorrect = cetificateReview.IsURLLinkToServiceCorrect;
                    existingEntity.IsGPG44Correct = cetificateReview.IsGPG44Correct;
                    existingEntity.IsGPG45Correct = cetificateReview.IsGPG45Correct;
                    existingEntity.IsServiceProvisionCorrect = cetificateReview.IsServiceProvisionCorrect;
                    existingEntity.IsLocationCorrect = cetificateReview.IsLocationCorrect;
                    existingEntity.IsDateOfIssueCorrect = cetificateReview.IsDateOfIssueCorrect;
                    existingEntity.IsDateOfExpiryCorrect = cetificateReview.IsDateOfExpiryCorrect;
                    existingEntity.IsAuthenticyVerifiedCorrect = cetificateReview.IsAuthenticyVerifiedCorrect;
                    existingEntity.CommentsForIncorrect = cetificateReview.CommentsForIncorrect;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.CertificateReviewStatus = cetificateReview.CertificateReviewStatus;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CertificateReview, loggedInUserEmail);
                }

                else
                {
                    cetificateReview.CreatedDate = DateTime.UtcNow;
                    var entity = await context.CertificateReview.AddAsync(cetificateReview);
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CertificateReview, loggedInUserEmail);
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

        // Part 2 of 2: Information match  Approve submission and Save as draft flow
        public async Task<GenericResponse> UpdateCertificateReview(CertificateReview cetificateReview, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.Include(p => p.CertificateReviewRejectionReasonMapping).FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId  && e.ProviProviderProfileId ==cetificateReview.ProviProviderProfileId);

                if (existingEntity != null)
                {
                    //clear rejection reasons if already exists for approval 
                    if (existingEntity.CertificateReviewRejectionReasonMapping != null && cetificateReview.CertificateReviewStatus == CertificateReviewEnum.Approved)
                    {
                        context.CertificateReviewRejectionReasonMapping.RemoveRange(existingEntity.CertificateReviewRejectionReasonMapping);
                        existingEntity.RejectionComments = null;
                    }

                    existingEntity.InformationMatched = cetificateReview.InformationMatched;
                    existingEntity.Comments = cetificateReview.Comments;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.CertificateReviewStatus = cetificateReview.CertificateReviewStatus;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CertificateReview, loggedInUserEmail);
                    transaction.Commit();
                    genericResponse.Success = true;
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

        //// Part 2 of 2: Information match  Reject submission flow
        public async Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReview cetificateReview, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.Include(p => p.CertificateReviewRejectionReasonMapping).FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId  && e.ProviProviderProfileId ==cetificateReview.ProviProviderProfileId);

                if (existingEntity != null)
                {
                    existingEntity.Comments = cetificateReview.Comments;
                    existingEntity.InformationMatched = cetificateReview.InformationMatched;
                    existingEntity.CertificateReviewStatus = cetificateReview.CertificateReviewStatus;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.RejectionComments = cetificateReview.RejectionComments;

                    //clear rejection reasons if already exists and update with reasons selected
                    if (existingEntity.CertificateReviewRejectionReasonMapping != null & existingEntity.CertificateReviewRejectionReasonMapping?.Count >0)
                        context.CertificateReviewRejectionReasonMapping.RemoveRange(existingEntity.CertificateReviewRejectionReasonMapping);

                    existingEntity.CertificateReviewRejectionReasonMapping = cetificateReview.CertificateReviewRejectionReasonMapping;

                    foreach (var mapping in cetificateReview.CertificateReviewRejectionReasonMapping)
                    {
                        context.Entry(mapping).State = EntityState.Added;
                    }
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CertificateReview, loggedInUserEmail);
                    transaction.Commit();
                    genericResponse.Success = true;
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

        //// Part 2 of 2: Information match Send back to CAB flow
        public async Task<GenericResponse> UpdateCertificateSentBack(CertificateReview cetificateReview, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview
                    .FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId && e.ProviProviderProfileId == cetificateReview.ProviProviderProfileId);

                if (existingEntity != null)
                {
                    existingEntity.Comments = cetificateReview.Comments;
                    existingEntity.InformationMatched = cetificateReview.InformationMatched;
                    existingEntity.CertificateReviewStatus = cetificateReview.CertificateReviewStatus;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.Amendments = cetificateReview.Amendments;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                }

                var existingService = await context.Service
                    .FirstOrDefaultAsync(e => e.Id == cetificateReview.ServiceId);

                if (existingService != null)
                {
                    existingService.ServiceStatus = ServiceStatusEnum.AmendmentsRequired;
                    existingService.ModifiedTime = DateTime.UtcNow;
                }

                await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CertificateReview, loggedInUserEmail);
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


        //Restore from archive tab
        public async Task<GenericResponse> RestoreRejectedCertificateReview(int reviewId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.FirstOrDefaultAsync(e => e.Id == reviewId);
                if (existingEntity != null)
                {
                    existingEntity.CertificateReviewStatus = CertificateReviewEnum.InReview;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.RestoreCertificateReview, loggedInUserEmail);
                    transaction.Commit();
                    genericResponse.Success = true;
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
        

        #endregion
    }
}
