using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.Data.Repositories
{
    public class EditRepository : IEditRepository
    {
        private readonly DVSAdminDbContext _context;
        private readonly ILogger<EditRepository> _logger;

        public EditRepository(DVSAdminDbContext context, ILogger<EditRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Service> GetService(int serviceId)
        {
            return await _context.Service
            .Include(s => s.Provider)
            .Include(s => s.CertificateReview)
            .Include(s => s.ServiceSupSchemeMapping)
            .ThenInclude(s => s.SupplementaryScheme)
            .Include(s => s.ServiceRoleMapping)
            .ThenInclude(s => s.Role)
            .Include(s => s.ServiceQualityLevelMapping)
            .ThenInclude(s => s.QualityLevel)
            .Include(s => s.ServiceIdentityProfileMapping)
            .ThenInclude(s => s.IdentityProfile)
            .FirstOrDefaultAsync(s => s.Id == serviceId);
        }
        public async Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail)
        {
            var response = new GenericResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingDraft = await _context.ProviderProfileDraft
                    .FirstOrDefaultAsync(x => x.ProviderProfileId == draft.ProviderProfileId && x.Id == draft.Id);

                var provider =await  _context.ProviderProfile.Include(p=>p.Services).FirstOrDefaultAsync(x => x.Id == draft.ProviderProfileId);

                if (existingDraft != null)
                {
                    UpdateExistingProviderDraft(draft, existingDraft);
                    await _context.SaveChangesAsync();

                    response.InstanceId = existingDraft.Id;
                }
                else
                {
                    draft.ModifiedTime = DateTime.UtcNow;
                    await _context.ProviderProfileDraft.AddAsync(draft);
                    var publishedServices = provider?.Services?.Where(x => x.IsCurrent == true && x.ServiceStatus == ServiceStatusEnum.Published);
                    if(publishedServices!=null && publishedServices.Count() > 0) 
                    {
                        foreach (var service in publishedServices)
                        {
                            service.ServiceStatus = ServiceStatusEnum.UpdatesRequested;
                            service.ModifiedTime = DateTime.UtcNow;
                            ServiceDraft serviceDraft = new()
                            {
                                ModifiedTime = DateTime.UtcNow,
                                PreviousServiceStatus = service.ServiceStatus,
                                RequestedUserId = draft.RequestedUserId,
                                ProviderProfileId = draft.ProviderProfileId

                            };
                        }
                    }                  
                    await _context.SaveChangesAsync(TeamEnum.DSIT,EventTypeEnum.DSITEditProvider,loggedInUserEmail);
                    response.InstanceId = draft.Id;
                }

                transaction.Commit();
                response.Success = true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                response.Success = false;
                _logger.LogError(ex, "Error in SaveProviderDraft");
            }
            return response;
        }    

        public async Task<GenericResponse> SaveServiceDraft(ServiceDraft draft, string loggedInUserEmail)
        {
            var response = new GenericResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingDraft = await _context.ServiceDraft.FirstOrDefaultAsync(x => x.ServiceId == draft.ServiceId &&x.Id == draft.Id);

                if (existingDraft != null)
                {
                    UpdateExistingServiceDraft(draft, existingDraft);
                    await _context.SaveChangesAsync();
                    response.InstanceId = existingDraft.Id;
                }
                else
                {
                    draft.ModifiedTime = DateTime.UtcNow;
                    await _context.ServiceDraft.AddAsync(draft);
                    await _context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.DSITEditService, loggedInUserEmail);
                    response.InstanceId = draft.Id;
                }

                transaction.Commit();
                response.Success = true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                response.Success = false;
                _logger.LogError(ex, "Error in SaveServiceDraft");
            }
            return response;
        }


       

        public async Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId)
        {
            var existingProvider = await _context.ProviderProfile.FirstOrDefaultAsync(p => p.RegisteredName.ToLower() == registeredName.ToLower() && p.Id != providerId);

            if (existingProvider != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateExistingServiceDraft(ServiceDraft source, ServiceDraft target)
        {
            target.ServiceName = source.ServiceName;
            target.WebSiteAddress = source.WebSiteAddress;
            target.CompanyAddress = source.CompanyAddress;
            
            if (target.ServiceRoleMappingDraft != null && target.ServiceRoleMappingDraft.Count > 0)
            {
                _context.ServiceRoleMappingDraft.RemoveRange(target.ServiceRoleMappingDraft);
            }
            target.ServiceRoleMappingDraft = source.ServiceRoleMappingDraft;

            if (target.ServiceIdentityProfileMappingDraft != null && target.ServiceIdentityProfileMappingDraft.Count > 0)
            {
                _context.ServiceIdentityProfileMappingDraft.RemoveRange(target.ServiceIdentityProfileMappingDraft);
            }
            target.ServiceIdentityProfileMappingDraft = source.ServiceIdentityProfileMappingDraft;

            if (target.ServiceQualityLevelMappingDraft != null && target.ServiceQualityLevelMappingDraft.Count > 0)
            {
                _context.ServiceQualityLevelMappingDraft.RemoveRange(target.ServiceQualityLevelMappingDraft);
            }
            target.ServiceQualityLevelMappingDraft = source.ServiceQualityLevelMappingDraft;

            target.HasSupplementarySchemes = source.HasSupplementarySchemes;
            target.HasGPG44 = source.HasGPG44;
            target.HasGPG45 = source.HasGPG45;

            if (target.ServiceSupSchemeMappingDraft != null && target.ServiceSupSchemeMappingDraft.Count > 0)
            {
                _context.ServiceSupSchemeMappingDraft.RemoveRange(target.ServiceSupSchemeMappingDraft);
            }
            target.ServiceSupSchemeMappingDraft = source.ServiceSupSchemeMappingDraft;

            target.ConformityIssueDate = source.ConformityIssueDate;
            target.ConformityExpiryDate = source.ConformityExpiryDate;
            target.PreviousServiceStatus = source.PreviousServiceStatus;

            target.ModifiedTime = DateTime.UtcNow;
        }
        private void UpdateExistingProviderDraft(ProviderProfileDraft source, ProviderProfileDraft target)
        {
            target.ModifiedTime = DateTime.UtcNow;
            target.RegisteredName = source.RegisteredName;
            target.TradingName = source.TradingName;
            target.HasRegistrationNumber = source.HasRegistrationNumber;
            target.CompanyRegistrationNumber = source.CompanyRegistrationNumber;
            target.DUNSNumber = source.DUNSNumber;
            target.HasParentCompany = source.HasParentCompany;
            target.ParentCompanyRegisteredName = source.ParentCompanyRegisteredName;
            target.ParentCompanyLocation = source.ParentCompanyLocation;
            target.PrimaryContactFullName = source.PrimaryContactFullName;
            target.PrimaryContactJobTitle = source.PrimaryContactJobTitle;
            target.PrimaryContactEmail = source.PrimaryContactEmail;
            target.PrimaryContactTelephoneNumber = source.PrimaryContactTelephoneNumber;
            target.SecondaryContactFullName = source.SecondaryContactFullName;
            target.SecondaryContactJobTitle = source.SecondaryContactJobTitle;
            target.SecondaryContactEmail = source.SecondaryContactEmail;
            target.SecondaryContactTelephoneNumber = source.SecondaryContactTelephoneNumber;
            target.PublicContactEmail = source.PublicContactEmail;
            target.ProviderTelephoneNumber = source.ProviderTelephoneNumber;
            target.ProviderWebsiteAddress = source.ProviderWebsiteAddress;
            target.PreviousProviderStatus = source.PreviousProviderStatus;
        }



        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            ProviderProfile providerProfile = new();
            providerProfile = await _context.ProviderProfile         
           .Where(p => p.Id == providerId && p.ProviderStatus >= ProviderStatusEnum.Published && p.ProviderStatus != ProviderStatusEnum.RemovedFromRegister)
           .OrderBy(c => c.ModifiedTime).FirstOrDefaultAsync() ?? new ProviderProfile();
            return providerProfile;
        }
    }
}
