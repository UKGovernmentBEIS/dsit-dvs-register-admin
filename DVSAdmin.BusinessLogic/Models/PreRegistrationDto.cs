﻿using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Models.PreRegistration
{
    public class PreRegistrationDto
    {
        public bool IsApplicationSponsor { get; set; }

        public string FullName { get; set; }
        public string JobTitle { get; set; }      
        public string Email { get; set; }

        public string TelephoneNumber { get; set; }
        public string? SponsorFullName { get; set; }
        public string? SponsorJobTitle { get; set; }
      
        public string? SponsorEmail { get; set; }
        public string? SponsorTelephoneNumber { get; set; }

        public ICollection<PreRegistrationCountryMappingDto> PreRegistrationCountryMappings { get; set; }
    
        public string RegisteredCompanyName { get; set; }

        public string TradingName { get; set; }      
        public string CompanyRegistrationNumber { get; set; }
        public bool HasParentCompany { get; set; }    
        public string? ParentCompanyRegisteredName { get; set; }
      
        public string? ParentCompanyLocation { get; set; }
        public YesNoEnum ConfirmAccuracy { get; set; }
        public string? URN { get; set; }
        public PreRegistrationStatusEnum PreRegistrationStatus { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int PrimaryChecksCommentCount { get; set; }
        public int SecondaryChecksCommentCount { get; set; }
        public int DaysLeftToComplete { get; set; }
    }
}
