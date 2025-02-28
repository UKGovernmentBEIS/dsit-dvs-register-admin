﻿using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Validations;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class ServiceSummaryViewModel
    {
        public ProviderProfileDto Provider { get; set; }

        [Required(ErrorMessage = "Enter the service name")]
        [MaximumLength(160, ErrorMessage = "The service name must be less than 161 characters")]
        [AcceptedCharacters(@"^[A-Za-z0-9 &@#().:-]+$", ErrorMessage = "The service name must contain only letters, numbers and accepted characters")]
        public string? ServiceName { get; set; }

        [Required(ErrorMessage = "Enter the service website address")]
        [RegularExpression(@"^(https?:\/\/)?(www\.)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(\/[^\s]*)?$", ErrorMessage = "Enter a valid website address")]
        public string? ServiceURL { get; set; }

        [Required(ErrorMessage = "Enter the company address")]
        public string? CompanyAddress { get; set; }
        public RoleViewModel? RoleViewModel { get; set; }

        [Required(ErrorMessage = "Select ‘Yes’ if the service is certified against GPG44")]
        public bool? HasGPG44 { get; set; }

        [Required(ErrorMessage = "Select ‘Yes’ if the service is certified against GPG45")]
        public bool? HasGPG45 { get; set; }
        public QualityLevelViewModel? QualityLevelViewModel { get; set; }
        public IdentityProfileViewModel? IdentityProfileViewModel { get; set; }

        [Required(ErrorMessage = "Select ‘Yes’ if the service is certified against a supplementary scheme")]
        public bool? HasSupplementarySchemes { get; set; }
        public SupplementarySchemeViewModel? SupplementarySchemeViewModel { get; set; }
        public string? FileName { get; set; }
        public string? FileLink { get; set; }
        public decimal? FileSizeInKb { get; set; }
        public DateTime? ConformityIssueDate { get; set; }
        public DateTime? ConformityExpiryDate { get; set; }
        public int CabUserId { get; set; }
        public int CabId { get; set; }
        public bool FromSummaryPage { get; set; }
        public int ServiceId { get; set; }
        public int ServiceKey { get; set; }


        public void ResetInpuData()
        {
            ServiceName = null;
            ServiceURL = null;
            CompanyAddress = null;
            ServiceId = 0;

            QualityLevelViewModel = new QualityLevelViewModel
            {
                SelectedLevelOfProtections = new List<QualityLevelDto>(),
                SelectedQualityofAuthenticators = new List<QualityLevelDto>()
            };

            RoleViewModel = new RoleViewModel
            {
                SelectedRoles = new List<RoleDto>()
            };

            IdentityProfileViewModel = new IdentityProfileViewModel
            {
                SelectedIdentityProfiles = new List<IdentityProfileDto>()
            };

            SupplementarySchemeViewModel = new SupplementarySchemeViewModel
            {
                SelectedSupplementarySchemes = new List<SupplementarySchemeDto>()
            };

            HasSupplementarySchemes = null;
            HasGPG44 = null;
            HasGPG45 = null;

            FileLink = null;
            FileName = null;
            FileSizeInKb = null;
            ConformityIssueDate = null;
            ConformityExpiryDate = null;
        }
    }
}
