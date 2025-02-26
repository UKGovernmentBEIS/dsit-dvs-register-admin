﻿using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IEditService
    {
        public Task<GenericResponse> SaveProviderDraft(ProviderProfileDraftDto draftDto, string loggedInUserEmail, List<string> dsitUserEmails);
        public Task<GenericResponse> SaveServiceDraft(ServiceDraftDto draftDto, string loggedInUserEmail);
        public Task<ServiceDto> GetService(int serviceId);
        public Task<bool> CheckProviderRegisteredNameExists(string registeredName, int providerId);
        public Task<ProviderProfileDto> GetProviderDeatils(int providerId);
        public Task<List<RoleDto>> GetRoles();
        public Task<List<QualityLevelDto>> GetQualitylevels();
        public Task<List<IdentityProfileDto>> GetIdentityProfiles();
        public Task<List<SupplementarySchemeDto>> GetSupplementarySchemes();
    }
}
