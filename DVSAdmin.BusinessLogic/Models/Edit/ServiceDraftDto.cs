﻿using DVSAdmin.CommonUtility.Models;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceDraftDto
    {
        public int Id { get; set; }      
        public int serviceId { get; set; }
        public string? ServiceName { get; set; }
        public string? CompanyAddress { get; set; }
        public bool? HasGPG44 { get; set; }
        public bool? HasGPG45 { get; set; }
        public bool? HasSupplementarySchemes { get; set; }
        public DateTime? ConformityIssueDate { get; set; }
        public DateTime? ConformityExpiryDate { get; set; }
        public ServiceStatusEnum PreviousServiceStatus { get; set; }
        public int ProviderProfileId { get; set; }
        public ProviderProfileDto Provider { get; set; }

        public ICollection<ServiceRoleMappingDraftDto> ServiceRoleMappingDraft { get; set; } = new List<ServiceRoleMappingDraftDto>();
        public ICollection<ServiceQualityLevelMappingDraftDto> ServiceQualityLevelMappingDraft { get; set; } = new List<ServiceQualityLevelMappingDraftDto>();
        public ICollection<ServiceIdentityProfileMappingDraftDto> ServiceIdentityProfileMappingDraft { get; set; } = new List<ServiceIdentityProfileMappingDraftDto>();
        public ICollection<ServiceSupSchemeMappingDraftDto> ServiceSupSchemeMappingDraft { get; set; } = new List<ServiceSupSchemeMappingDraftDto>();

    }
}