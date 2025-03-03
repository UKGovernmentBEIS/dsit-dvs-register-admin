﻿using System.ComponentModel.DataAnnotations;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DVSAdmin.Data.Entities
{
    public class ProviderProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string RegisteredName { get; set; }
        public string TradingName { get; set; }
        public bool HasRegistrationNumber { get; set; }
        public string? CompanyRegistrationNumber { get; set; }
        public string? DUNSNumber { get; set; }
        public bool HasParentCompany { get; set; }
        public string? ParentCompanyRegisteredName { get; set; }
        public string? ParentCompanyLocation { get; set; }
        public string PrimaryContactFullName { get; set; }
        public string PrimaryContactJobTitle { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string PrimaryContactTelephoneNumber { get; set; }
        public string SecondaryContactFullName { get; set; }
        public string SecondaryContactJobTitle { get; set; }
        public string SecondaryContactEmail { get; set; }
        public string SecondaryContactTelephoneNumber { get; set; }
        public string PublicContactEmail { get; set; }
        public string? ProviderTelephoneNumber { get; set; }
        public string ProviderWebsiteAddress { get; set; }
        public RemovalReasonsEnum? RemovalReason { get; set; }

        [ForeignKey("CabUser")]
        public int CabUserId { get; set; }
        public CabUser CabUser { get; set; }
        public ProviderStatusEnum ProviderStatus { get; set; }
        public ICollection<Service>? Services { get; set; }      
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? PublishedTime { get; set; }
        public DateTime? RemovalRequestTime { get; set; }
        public PublicInterestCheck PublicInterestCheck { get; set; }
        public DateTime? RemovedTime { get; set; }
    }
}
