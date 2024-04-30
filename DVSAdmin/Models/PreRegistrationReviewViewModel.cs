using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    

    public class PreRegistrationReviewViewModel
    {
        public PreRegistrationDto? PreRegistration { get; set; }
        public int PreRegistrationId { get; set; }


        [Required(ErrorMessage ="Select an option")]
        public bool? IsCountryApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsCompanyApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsCheckListApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsDirectorshipsApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsDirectorshipsAndRelationApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsTradingAddressApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsSanctionListApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsUNFCApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsECCheckApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsTARICApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsBannedPoliticalApproved { get; set; }

        [Required(ErrorMessage = "Select an option")]
        public bool? IsProvidersWebpageApproved { get; set; }       
        public ReviewTypeEnum ReviewType { get; set; }        
        public ApplicationReviewStatusEnum ApplicationReviewStatus { get; set; }
       
        public string? Comment { get; set; }    
        public string? SubmitValidation { get; set; }

       



    }
}
