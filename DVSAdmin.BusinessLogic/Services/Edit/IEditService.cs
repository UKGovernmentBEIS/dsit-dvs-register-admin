using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IEditService
    {
        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail, List<string> dsitUserEmails);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail, List<string> dsitUserEmails);
        public Task<ServiceDto> GetService(int serviceId);
        public Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId);
        public Task<ProviderProfileDto> GetProviderDetails(int providerId);
        public Task<(Dictionary<string, List<string>>, Dictionary<string, List<string>>)> GetServiceKeyValue(ServiceDraftDto currentData, ServiceDto previousData);
        public Task<List<RoleDto>> GetRoles(decimal tfVersion);
        public Task<List<QualityLevelDto>> GetQualitylevels();
        public Task<List<IdentityProfileDto>> GetIdentityProfiles();
        public Task<List<SupplementarySchemeDto>> GetSupplementarySchemes();
        public Task<GenericResponse> GenerateTokenAndSendServiceUpdateRequest(ServiceDraftDto? draftDto, string loggedInUserEmail, List<string> dsitUserEmails, int serviceDraftId, bool isResend);
        public Task<IReadOnlyList<CabDto>> GetAllCabs();
        public Task<List<ServiceDto>> GetPublishedUnderpinningServices(string SearchText, int? currentSelectedServiceId);
        public Task<List<ServiceDto>> GetServicesWithManualUnderinningService(string searchText, int? currentSelectedServiceId);
    }
}
