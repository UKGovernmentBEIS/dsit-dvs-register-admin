using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
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

        [JsonIgnore]
        public ICollection<ServiceRoleMappingDto> ServiceRoleMapping { get; set; }
        public bool? HasGPG44 { get; set; }
        [JsonIgnore]
        public ICollection<ServiceQualityLevelMappingDto>? ServiceQualityLevelMapping { get; set; }
        public bool? HasGPG45 { get; set; }
        [JsonIgnore]
        public ICollection<ServiceIdentityProfileMappingDto>? ServiceIdentityProfileMapping { get; set; }
        public bool? HasSupplementarySchemes { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
        public ICollection<CabTransferRequestDto> CabTransferRequest { get; set; }
        public int CabTransferRequestId { get; set; }

        public int TrustFrameworkVersionId { get; set; }
        [JsonIgnore]
        public TrustFrameworkVersionDto TrustFrameworkVersion { get; set; }
        public ServiceTypeEnum? ServiceType { get; set; }
        public int? UnderPinningServiceId { get; set; }
        [JsonIgnore]
        public ServiceDto UnderPinningService { get; set; }
        public int? ManualUnderPinningServiceId { get; set; }
        [JsonIgnore]
        public ManualUnderPinningServiceDto ManualUnderPinningService { get; set; }
        
        public bool? IsManualServiceLinkedToMultipleServices { get; set; }
        public bool? IsUnderPinningServicePublished { get; set; }

        public string NewOrResubmission { get; set; }
        public bool? IsResubmission { get; set; }
        public int? PreviousVersionServiceId { get; set; }
    }
}
