﻿using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using System.Text.Json.Serialization;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceDto
    {       
        public int Id { get; set; }
        public int ProviderProfileId { get; set; }
        public ProviderProfileDto Provider { get; set; }
        public string ServiceName { get; set; }
        public int ServiceKey { get; set; }
        public int ServiceVersion { get; set; } = 1;
        public bool IsCurrent { get; set; } = true;
        public string CompanyAddress { get; set; }
        public string WebSiteAddress { get; set; }
        public ICollection<ServiceRoleMappingDto> ServiceRoleMapping { get; set; }
        public bool? HasGPG44 { get; set; }     
        public ICollection<ServiceQualityLevelMappingDto>? ServiceQualityLevelMapping { get; set; }
        public bool? HasGPG45 { get; set; }    
        public ICollection<ServiceIdentityProfileMappingDto>? ServiceIdentityProfileMapping { get; set; }
        public bool? HasSupplementarySchemes { get; set; }   
        public ICollection<ServiceSupSchemeMappingDto>? ServiceSupSchemeMapping { get; set; }

        public string? FileName { get; set; }
        public string? FileLink { get; set; }        
        public decimal? FileSizeInKb { get; set; }
        public DateTime? ConformityIssueDate { get; set; }
        public DateTime? ConformityExpiryDate { get; set; }      
        public int CabUserId { get; set; }
        public CabUserDto CabUser { get; set; }
        public int TrustMarkNumber { get; set; }
        public ServiceStatusEnum ServiceStatus { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? PublishedTime { get; set; }   
        public CertificateReviewDto CertificateReview { get; set; }
        public PublicInterestCheckDto PublicInterestCheck { get; set; }
        public string? RemovalReasonByCab { get; set; }
        public int DaysLeftToComplete { get; set; }
        public int DaysLeftToCompletePICheck { get; set; }

        [JsonIgnore]
        public ProceedApplicationConsentTokenDto ProceedApplicationConsentToken { get; set; }

        [JsonIgnore]
        public ProceedPublishConsentTokenDto ProceedPublishConsentToken { get; set; }       
        public ServiceRemovalReasonEnum ServiceRemovalReason { get; set; }
        public bool HasPreviousPublishedVersion { get; set; }
        public DateTime? ResubmissionTime { get; set; }

        [JsonIgnore]
        public ServiceDraftDto ServiceDraft { get; set; }
    }
}
