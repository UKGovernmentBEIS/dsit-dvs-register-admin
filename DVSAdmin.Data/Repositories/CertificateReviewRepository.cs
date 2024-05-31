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
                var existingEntity = await context.CetificateReview.FirstOrDefaultAsync(e => e.CertificateInformationId == cetificateReview.CertificateInformationId && e.PreRegistrationId == cetificateReview.PreRegistrationId);

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
                    existingEntity.IsIdentityProfilesCorrect = cetificateReview.IsIdentityProfilesCorrect;
                    existingEntity.IsQualityAssessmentCorrect = cetificateReview.IsQualityAssessmentCorrect;
                    existingEntity.IsServiceProvisionCorrect = cetificateReview.IsServiceProvisionCorrect;
                    existingEntity.IsLocationCorrect = cetificateReview.IsLocationCorrect;
                    existingEntity.IsDateOfIssueCorrect = cetificateReview.IsDateOfIssueCorrect;
                    existingEntity.IsDateOfExpiryCorrect = cetificateReview.IsDateOfExpiryCorrect;
                    existingEntity.IsAuthenticyVerifiedCorrect = cetificateReview.IsAuthenticyVerifiedCorrect;
                    existingEntity.CommentsForIncorrect = cetificateReview.CommentsForIncorrect;
                    existingEntity.InformationMatched = cetificateReview.InformationMatched;
                    existingEntity.Comments = cetificateReview.Comments;
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.CertificateInfoStatus = cetificateReview.CertificateInfoStatus;
                    if(cetificateReview.CertificateInfoStatus == CertificateInfoStatusEnum.Rejected)
                    existingEntity.CertificateReviewRejectionReasonMappings = cetificateReview.CertificateReviewRejectionReasonMappings;
                    cetificateReview.ModifiedDate = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                    genericResponse.InstanceId = existingEntity.Id;

                }
                else
                {
                    cetificateReview.CreatedDate = DateTime.UtcNow;
                    var entity = await context.CetificateReview.AddAsync(cetificateReview);
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

        public async Task<List<CertificateInformation>> GetCertificateInformationList()
        {
            return await context.CertificateInformation.Include(p=>p.CertificateInfoRoleMapping).Include(p => p.CertificateReview).OrderBy(c => c.CreatedDate).ToListAsync();
        }

        public async Task<CertificateInformation> GetCertificateInformation(int certificateInfoId)
        {
            CertificateInformation certificateInformation = new CertificateInformation();
            certificateInformation = await context.CertificateInformation
            .Include(p => p.CertificateInfoIdentityProfileMapping)
            .Include(p => p.CertificateInfoRoleMapping)
            .Include(p => p.CertificateInfoSupSchemeMappings)
            .Include(p=>p.CertificateReview)
            .Where(p => p.Id == certificateInfoId).FirstOrDefaultAsync()?? new CertificateInformation();
            return certificateInformation;
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
    }
}
