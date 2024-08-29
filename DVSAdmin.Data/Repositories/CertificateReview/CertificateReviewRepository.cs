using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public async Task<GenericResponse> SaveCertificateReview(CertificateReview cetificateReview)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId && e.ProviProviderProfileId ==cetificateReview.ProviProviderProfileId);

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
                    //existingEntity.IsIdentityProfilesCorrect = cetificateReview.IsIdentityProfilesCorrect;
                    //existingEntity.IsQualityAssessmentCorrect = cetificateReview.IsQualityAssessmentCorrect;
                    existingEntity.IsServiceProvisionCorrect = cetificateReview.IsServiceProvisionCorrect;
                    existingEntity.IsLocationCorrect = cetificateReview.IsLocationCorrect;
                    existingEntity.IsDateOfIssueCorrect = cetificateReview.IsDateOfIssueCorrect;
                    existingEntity.IsDateOfExpiryCorrect = cetificateReview.IsDateOfExpiryCorrect;
                    existingEntity.IsAuthenticyVerifiedCorrect = cetificateReview.IsAuthenticyVerifiedCorrect;
                    existingEntity.CommentsForIncorrect = cetificateReview.CommentsForIncorrect;                   
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                   // existingEntity.CertificateInfoStatus = cetificateReview.CertificateInfoStatus;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;                    
                    await context.SaveChangesAsync();
                }
                
                else
                {
                    cetificateReview.CreatedDate = DateTime.UtcNow;
                    var entity = await context.CertificateReview.AddAsync(cetificateReview);
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
        public async Task<GenericResponse> UpdateCertificateReview(CertificateReview cetificateReview)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId  && e.ProviProviderProfileId ==cetificateReview.ProviProviderProfileId);

                if (existingEntity != null)
                {
                    existingEntity.InformationMatched = cetificateReview.InformationMatched;
                    existingEntity.Comments = cetificateReview.Comments;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    //existingEntity.CertificateInfoStatus = cetificateReview.CertificateInfoStatus;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                    await context.SaveChangesAsync();
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

        public async Task<GenericResponse> UpdateCertificateReviewRejection(CertificateReview cetificateReview)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingEntity = await context.CertificateReview.FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId  && e.ProviProviderProfileId ==cetificateReview.ProviProviderProfileId);

                if (existingEntity != null)
                {
                    existingEntity.InformationMatched = cetificateReview.InformationMatched;
                   // existingEntity.CertificateInfoStatus = cetificateReview.CertificateInfoStatus;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.RejectionComments = cetificateReview.RejectionComments;
                    //existingEntity.CertificateReviewRejectionReasonMappings = cetificateReview.CertificateReviewRejectionReasonMappings;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                    await context.SaveChangesAsync();
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

        public async Task<List<CertificateInformation>> GetCertificateInformationList()
        {
            return await context.CertificateInformation.Include(p=>p.CertificateInfoRoleMapping)
            .Include(p => p.CertificateInfoIdentityProfileMapping)
            .Include(p => p.CertificateInfoSupSchemeMappings)
            .Include(p => p.CertificateReview).Include(p => p.Provider).OrderBy(c => c.CreatedDate).ToListAsync();
        }

        public async Task<List<Service>> GetServiceList()
        {
            return await context.Service
            .Include(s => s.Provider)
            .Include(s => s.CertificateReview)
            .OrderBy(s => s.CreatedTime)
            .ToListAsync();
        }
        public async Task<List<CertificateInformation>> GetCertificateInformationListByProvider(int providerId)
        {
            return await context.CertificateInformation.Where(p => p.ProviderId == providerId && 
            (p.CertificateInfoStatus == CertificateInfoStatusEnum.ReadyToPublish || p.CertificateInfoStatus == CertificateInfoStatusEnum.Published))
            .ToListAsync()??new List<CertificateInformation>();
        }

        public async Task<CertificateInformation> GetCertificateInformation(int certificateInfoId)
        {
            CertificateInformation certificateInformation = new CertificateInformation();
            certificateInformation = await context.CertificateInformation
            .Include(p => p.CertificateInfoIdentityProfileMapping)
            .Include(p => p.CertificateInfoRoleMapping)
            .Include(p => p.CertificateInfoSupSchemeMappings)
            .Include(p=>p.CertificateReview)
            .Include(p => p.Provider)
             .Include(p => p.Provider.PreRegistration)
            .Where(p => p.Id == certificateInfoId).FirstOrDefaultAsync()?? new CertificateInformation();
            return certificateInformation;
        }

        public async Task<CertificateReview> GetCertificateReview(int reviewId)
        {
            CertificateReview certificateReview = new CertificateReview();
            certificateReview = await context.CertificateReview
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


        public async Task<GenericResponse> UpdateCertificateReviewStatus(int certificateReviewId, string modifiedBy, ProviderStatusEnum providerStatus)
        {
            GenericResponse genericResponse = new GenericResponse();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var reviewEntity = await context.CertificateReview.FirstOrDefaultAsync(e => e.Id == certificateReviewId);


                if(reviewEntity != null)                
                {
                    var certificateInfoEntity = await context.CertificateInformation.FirstOrDefaultAsync(e => e.Id == reviewEntity.ServiceId);
                    var providerEntity = await context.Provider.FirstOrDefaultAsync(e => e.Id == reviewEntity.ProviProviderProfileId);

                    if(certificateInfoEntity != null && providerEntity != null)
                    {
                        //update review table status so that it won't appear in review list again
                        //reviewEntity.CertificateInfoStatus = CertificateInfoStatusEnum.ReadyToPublish;
                        //reviewEntity.ModifiedBy = modifiedBy;
                        reviewEntity.ModifiedDate = DateTime.UtcNow;

                       
                        certificateInfoEntity.CertificateInfoStatus = CertificateInfoStatusEnum.ReadyToPublish;
                        certificateInfoEntity.ModifiedBy = modifiedBy;
                        certificateInfoEntity.ModifiedDate = DateTime.UtcNow;


                        providerEntity.ProviderStatus = providerStatus;
                        providerEntity.ModifiedTime = DateTime.UtcNow;

                        await context.SaveChangesAsync();
                        transaction.Commit();
                        genericResponse.Success = true;
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
    }
}
