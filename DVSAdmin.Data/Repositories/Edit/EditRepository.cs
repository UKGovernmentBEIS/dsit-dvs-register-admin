using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            .Include(s => s.Provider).AsNoTracking()
            .Include(s=>s.TrustFrameworkVersion).AsNoTracking()
            .Include(s=>s.UnderPinningService).ThenInclude(p=>p.Provider).AsNoTracking()
             .Include(s => s.UnderPinningService).ThenInclude(p => p.CabUser).ThenInclude(c=>c.Cab).AsNoTracking()
            .Include(s => s.ManualUnderPinningService).ThenInclude(p => p.Cab).AsNoTracking()
            .Include(s => s.CertificateReview).AsNoTracking()
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SupplementaryScheme).AsNoTracking()
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SchemeGPG44Mapping).ThenInclude(s=>s.QualityLevel).AsNoTracking()
            .Include(s => s.ServiceSupSchemeMapping).ThenInclude(s => s.SchemeGPG45Mapping).ThenInclude(s => s.IdentityProfile).AsNoTracking()
            .Include(s => s.ServiceRoleMapping).ThenInclude(s => s.Role).AsNoTracking()
            .Include(s => s.ServiceQualityLevelMapping).ThenInclude(s => s.QualityLevel).AsNoTracking()
            .Include(s => s.ServiceIdentityProfileMapping).ThenInclude(s => s.IdentityProfile).AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId);
        }
        public async Task<List<Role>> GetRoles()
        {
            return await _context.Role.OrderBy(c => c.Order).ToListAsync();
        }
        public async Task<List<QualityLevel>> QualityLevels()
        {
            return await _context.QualityLevel.ToListAsync();
        }
        public async Task<List<IdentityProfile>> GetIdentityProfiles()
        {
            return await _context.IdentityProfile.OrderBy(c => c.IdentityProfileName).ToListAsync();
        }

        public async Task<List<SupplementaryScheme>> GetSupplementarySchemes()
        {
            return await _context.SupplementaryScheme.OrderBy(c => c.Order).ToListAsync();
        }

        public async Task<ServiceDraft?> GetServiceDraft(int serviceId)
        {
            return await _context.ServiceDraft.Include(p => p.Provider).ThenInclude(p => p.ProviderProfileDraft).Where(s => s.Id == serviceId).FirstOrDefaultAsync();

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



        public async Task<ProviderProfile> GetProviderDetails(int providerId)
        {
            ProviderProfile providerProfile = new();
            providerProfile = await _context.ProviderProfile      
               .Include(p => p.Services)
               .Where(p => p.Id == providerId && p.ProviderStatus >= ProviderStatusEnum.ReadyToPublish )
               .OrderBy(c => c.ModifiedTime).FirstOrDefaultAsync() ?? new ProviderProfile();
            return providerProfile;
        }


        public async Task<GenericResponse> SaveProviderDraftToken(ProviderDraftToken providerDraftToken, string loggedinUserEmail, int providerProfileId)
        {
            GenericResponse genericResponse = new();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingEntity = await _context.ProviderDraftToken.FirstOrDefaultAsync(e => e.ProviderProfileDraftId == providerDraftToken.ProviderProfileDraftId);
                var provider = await _context.ProviderProfile.FirstOrDefaultAsync(e => e.Id == providerProfileId);
                if (existingEntity == null)
                {
                    provider.EditProviderTokenStatus = TokenStatusEnum.Requested;
                    await _context.ProviderDraftToken.AddAsync(providerDraftToken);
                    await _context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.DSITEditProvider, loggedinUserEmail);
                    transaction.Commit();
                    genericResponse.Success = true;
                }

            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                transaction.Rollback();
                _logger.LogError($"Failed SaveProviderDraftToken: {ex}");
            }
            return genericResponse;
        }


        public async Task<GenericResponse> SaveServiceDraftToken(ServiceDraftToken serviceDraftToken, string loggedinUserEmail, int serviceId)
        {
            GenericResponse genericResponse = new();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingEntity = await _context.ServiceDraftToken.FirstOrDefaultAsync(e => e.ServiceDraftId == serviceDraftToken.ServiceDraftId);
                var service = await _context.Service.FirstOrDefaultAsync(e => e.Id == serviceId);
                if (existingEntity == null)
                {
                    service.EditServiceTokenStatus = TokenStatusEnum.Requested;
                    serviceDraftToken.CreatedTime = DateTime.UtcNow;
                    await _context.ServiceDraftToken.AddAsync(serviceDraftToken);                  
                }
                else
                {
                    service.EditServiceTokenStatus = TokenStatusEnum.RequestResent;
                    existingEntity.Token = serviceDraftToken.Token;
                    existingEntity.TokenId = serviceDraftToken.TokenId;
                    existingEntity.ModifiedTime = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.DSITEditService, loggedinUserEmail);
                await transaction.CommitAsync();
                genericResponse.Success = true;

            }
            catch (Exception ex)
            {
                genericResponse.EmailSent = false;
                genericResponse.Success = false;
                await transaction.RollbackAsync();
                _logger.LogError($"Failed SaveProviderDraftToken: {ex}");
            }
            return genericResponse;
        }

        public async Task<GenericResponse> SaveProviderDraft(ProviderProfileDraft draft, string loggedInUserEmail)
        {
            var response = new GenericResponse();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Email == loggedInUserEmail);
                draft.RequestedUserId = user.Id;
                var existingDraft = await _context.ProviderProfileDraft
                    .FirstOrDefaultAsync(x => x.ProviderProfileId == draft.ProviderProfileId && x.Id == draft.Id);

                var provider = await _context.ProviderProfile.Include(p => p.Services).FirstOrDefaultAsync(x => x.Id == draft.ProviderProfileId);

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
                    var servicesList = provider?.Services?.Where(x => x.ServiceStatus == ServiceStatusEnum.Published || x.ServiceStatus == ServiceStatusEnum.ReadyToPublish);

                    provider.ProviderStatus = ProviderStatusEnum.UpdatesRequested;
                    provider.ModifiedTime = DateTime.UtcNow;
                    if (servicesList != null && servicesList.Count() > 0)
                    {
                        foreach (var service in servicesList)
                        {
                            ServiceDraft serviceDraft = new()
                            {
                                ModifiedTime = DateTime.UtcNow,
                                PreviousServiceStatus = service.ServiceStatus,
                                RequestedUserId = draft.RequestedUserId,
                                ProviderProfileId = draft.ProviderProfileId,
                                ServiceId = service.Id

                            };

                            await _context.ServiceDraft.AddAsync(serviceDraft);
                            service.ServiceStatus = ServiceStatusEnum.UpdatesRequested;
                            service.ModifiedTime = DateTime.UtcNow;



                        }
                    }
                    await _context.SaveChangesAsync(TeamEnum.DSIT, EventTypeEnum.DSITEditProvider, loggedInUserEmail);
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
                var user = await _context.User.FirstOrDefaultAsync(x => x.Email == loggedInUserEmail);
                draft.RequestedUserId = user.Id;
                var existingDraft = await _context.ServiceDraft.FirstOrDefaultAsync(x => x.ServiceId == draft.ServiceId && x.Id == draft.Id);

                var existingService = await _context.Service.Include(p => p.Provider).FirstOrDefaultAsync(x => x.Id == draft.ServiceId);
                if (existingDraft != null)
                {
                    UpdateExistingServiceDraft(draft, existingDraft);
                    await _context.SaveChangesAsync();
                    response.InstanceId = existingDraft.Id;
                }
                else
                {
                    draft.ModifiedTime = DateTime.UtcNow;
                    draft.PreviousServiceStatus = existingService.ServiceStatus;
                    await _context.ServiceDraft.AddAsync(draft);

                    existingService.ServiceStatus = ServiceStatusEnum.UpdatesRequested;
                    existingService.ModifiedTime = DateTime.UtcNow;
                    existingService.Provider.ModifiedTime = DateTime.UtcNow;
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


        #region Private methods
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
            target.RequestedUserId = source.RequestedUserId;
        }
        #endregion
    }
}
