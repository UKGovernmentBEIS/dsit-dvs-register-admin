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
            .Include(s=>s.Service).ThenInclude(s => s.Provider)
            .Include(s => s.Service).ThenInclude(s => s.CabUser).ThenInclude(s=>s.Cab)
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

       
        public async Task<List<Service>> GetServiceList(string searchText = "")
        {
            return await context.Service.Where(s => s.ServiceName.ToLower().Contains(searchText) ||
             s.Provider.RegisteredName.ToLower().Contains(searchText))
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
            .Include(p => p.TrustFrameworkVersion)
            .Include(p => p.UnderPinningService).ThenInclude(p=>p.Provider)
            .Include(p => p.UnderPinningService).ThenInclude(p => p.CabUser).ThenInclude(p=>p.Cab)
            .Include(p => p.ManualUnderPinningService).ThenInclude(x=>x.Cab)
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

        public async Task<Service> GetPreviousServiceVersion(int currentServiceId)
        {
            int previousVersion;
            Service previousServiceVersion = new();
            var currentVersionService = await context.Service.FirstOrDefaultAsync(x=>x.Id == currentServiceId);
            if(currentVersionService!=null && currentVersionService.ServiceVersion>1)
            {
                 previousVersion = currentVersionService.ServiceVersion - 1;
                previousServiceVersion= await context.Service.FirstOrDefaultAsync(x => x.ServiceKey == currentVersionService.ServiceKey && x.ServiceVersion == previousVersion)??new();
            }
            return previousServiceVersion;

        }





            #region Save, update

           
            public async Task<GenericResponse> SaveCertificateReview(CertificateReview cetificateReview, string loggedInUserEmail)
            {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {


                var existingEntity = await context.CertificateReview.Include(p => p.CertificateReviewRejectionReasonMapping).FirstOrDefaultAsync(e => e.ServiceId == cetificateReview.ServiceId && e.ProviProviderProfileId == cetificateReview.ProviProviderProfileId);

                if(existingEntity != null)
                {
                    if (existingEntity.CertificateReviewRejectionReasonMapping != null && cetificateReview.CertificateReviewStatus == CertificateReviewEnum.Approved)
                    {
                        context.CertificateReviewRejectionReasonMapping.RemoveRange(existingEntity.CertificateReviewRejectionReasonMapping);
                        existingEntity.RejectionComments = null;
                    }
                    if(cetificateReview.CertificateReviewStatus == CertificateReviewEnum.Rejected)
                    {
                        existingEntity.RejectionComments = cetificateReview.RejectionComments;

                        //clear rejection reasons if already exists and update with reasons selected
                        if (existingEntity.CertificateReviewRejectionReasonMapping != null & existingEntity.CertificateReviewRejectionReasonMapping?.Count > 0)
                            context.CertificateReviewRejectionReasonMapping.RemoveRange(existingEntity.CertificateReviewRejectionReasonMapping);

                        existingEntity.CertificateReviewRejectionReasonMapping = cetificateReview.CertificateReviewRejectionReasonMapping;

                        foreach (var mapping in cetificateReview.CertificateReviewRejectionReasonMapping)
                        {
                            context.Entry(mapping).State = EntityState.Added;
                        }
                    }
                    else if (cetificateReview.CertificateReviewStatus == CertificateReviewEnum.AmendmentsRequired)
                    {
                        existingEntity.Amendments = cetificateReview.Amendments;
                        var existingService = await context.Service
                      .FirstOrDefaultAsync(e => e.Id == cetificateReview.ServiceId);

                        if (existingService != null)
                        {
                            existingService.ServiceStatus = ServiceStatusEnum.AmendmentsRequired;
                            existingService.ModifiedTime = DateTime.UtcNow;
                        }
                    }


                    existingEntity.CertificateValid = cetificateReview.CertificateValid;
                    existingEntity.InformationMatched = cetificateReview.InformationMatched;               
                    existingEntity.VerifiedUser = cetificateReview.VerifiedUser;
                    existingEntity.CertificateReviewStatus = cetificateReview.CertificateReviewStatus;
                    existingEntity.ModifiedDate = DateTime.UtcNow;
                    genericResponse.InstanceId = existingEntity.Id;
                }

                else
                {
                    cetificateReview.CreatedDate = DateTime.UtcNow;
                    cetificateReview.ModifiedDate = DateTime.UtcNow;
                    var entity = await context.CertificateReview.AddAsync(cetificateReview);
                    genericResponse.InstanceId = entity.Entity.Id;
                    if (cetificateReview.CertificateReviewStatus == CertificateReviewEnum.AmendmentsRequired)
                    {
                        var existingService = await context.Service
                       .FirstOrDefaultAsync(e => e.Id == cetificateReview.ServiceId);

                        if (existingService != null)
                        {
                            existingService.ServiceStatus = ServiceStatusEnum.AmendmentsRequired;
                            existingService.ModifiedTime = DateTime.UtcNow;
                        }
                    }

                    await context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.CertificateReview, loggedInUserEmail);
                   
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

     


        //Restore from archive tab
        public async Task<GenericResponse> RestoreRejectedCertificateReview(int serviceId, string loggedInUserEmail)
        {
            GenericResponse genericResponse = new();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingService = await context.Service.Include(x => x.CertificateReview).ThenInclude(x=>x.CertificateReviewRejectionReasonMapping)
               .Where(x => x.Id == serviceId ).FirstOrDefaultAsync();



                if (existingService != null)
                {
                    existingService.ModifiedTime = DateTime.UtcNow;
                    existingService.ServiceStatus = ServiceStatusEnum.Resubmitted;
                    context.CertificateReview.Remove(existingService.CertificateReview);
                    genericResponse.InstanceId = existingService.Id;
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
