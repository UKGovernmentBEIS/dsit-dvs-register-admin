using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DVSAdmin.Models
{
    public class PublicInterestSecondaryCheckViewModel
    {        
       
        public int ServiceId { get; set; }
        public int ProviderProfileId { get; set; }

        [JsonIgnore]
        public ServiceDto? Service { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets Companies House or charity information number criteria")]
        public bool? IsCompanyHouseNumberApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets directorships criteria")]
        public bool? IsDirectorshipsApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets directors and relationships criteria")]
        public bool? IsDirectorshipsAndRelationApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets applicable trading as address checks criteria")]
        public bool? IsTradingAddressApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets sanctions list checks criteria")]
        public bool? IsSanctionListApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets applicable UNFC check criteria")]
        public bool? IsUNFCApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets applicable EC check criteria")]
        public bool? IsECCheckApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets applicable TARIC check criteria")]
        public bool? IsTARICApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets banned political affiliations criteria")]
        public bool? IsBannedPoliticalApproved { get; set; }

        [Required(ErrorMessage = "Select approve if the provider meets service provider’s website criteria")]
        public bool? IsProvidersWebpageApproved { get; set; }
        public ReviewTypeEnum ReviewType { get; set; }
        public PublicInterestCheckEnum PublicInterestCheckStatus { get; set; }

        [Required(ErrorMessage = "Enter comment")]
        public string? PrimaryCheckComment { get; set; }
        public string? SecondaryCheckComment { get; set; }
        public string? SubmitValidation { get; set; }
        public int? PrimaryCheckUserId { get; set; }
        public int? SecondaryCheckUserId { get; set; }

        [JsonIgnore]
        public PublicInterestCheckDto? PublicInterestCheck { get; set; }
    }
}
