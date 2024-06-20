using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class SecondaryCheckViewModel
    {
        public PreRegistrationDto? PreRegistration { get; set; }
        public int PreRegistrationId { get; set; }
        public bool? IsCountryApproved { get; set; }       
        public bool? IsCompanyApproved { get; set; }       
        public bool? IsCheckListApproved { get; set; }     
        public bool? IsDirectorshipsApproved { get; set; }       
        public bool? IsDirectorshipsAndRelationApproved { get; set; }       
        public bool? IsTradingAddressApproved { get; set; }       
        public bool? IsSanctionListApproved { get; set; }        
        public bool? IsUNFCApproved { get; set; }       
        public bool? IsECCheckApproved { get; set; }       
        public bool? IsTARICApproved { get; set; }      
        public bool? IsBannedPoliticalApproved { get; set; }        
        public bool? IsProvidersWebpageApproved { get; set; }
        public ReviewTypeEnum ReviewType { get; set; }
        public ApplicationReviewStatusEnum ApplicationReviewStatus { get; set; }

        
        public string? Comment { get; set; }

        
        public RejectionReasonEnum?  RejectionReason { get; set; }
        public string? SubmitValidation { get; set; }

        public int? PrimaryCheckUserId { get; set; }      
        public int? SecondaryCheckUserId { get; set; }
    }
}
