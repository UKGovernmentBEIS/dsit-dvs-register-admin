using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;       

namespace DVSAdmin.Data.Edit
{
    public class EditRepository
    {
        private readonly DVSAdminDbContext _context;
        private readonly ILogger<EditRepository> _logger;

        public EditRepository(DVSAdminDbContext context, ILogger<EditRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail)
        {
            var response = new GenericResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingDraft = await _context.ProviderProfileDraft
                    .FirstOrDefaultAsync(x => x.ProviderProfileId == draft.ProviderProfileId);

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
                    await _context.SaveChangesAsync();

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
                var existingDraft = await _context.ServiceDraft
                    .FirstOrDefaultAsync(x => x.ServiceId == draft.ServiceId);

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
                    await _context.SaveChangesAsync();

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
    }
}
