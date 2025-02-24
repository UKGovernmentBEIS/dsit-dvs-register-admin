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
                    // 3. Insert new record
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
            target.CurrentProviderStatus = source.CurrentProviderStatus;
        }
    }
}
