using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.PublicInterestCheck;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;
namespace DVSAdmin.Models
{
    public class PublicInterestSecondaryCheckViewModel
    {        
       
        public int ServiceId { get; set; }
        public int ProviderProfileId { get; set; }      
        public ServiceDto? Service { get; set; }

        public bool? PublicInterestChecksMet { get; set; }
        public ReviewTypeEnum ReviewType { get; set; }
        public PublicInterestCheckEnum PublicInterestCheckStatus { get; set; }

        public string? SubmitValidation { get; set; }
        public int? PrimaryCheckUserId { get; set; }
        public int? SecondaryCheckUserId { get; set; }
        public List<RejectionReasonDto>? AvailableReasons { get; set; } = new List<RejectionReasonDto>
        {
        new RejectionReasonDto { Id = 1, RejectionReasonName = "Failed due diligence check" },
        new RejectionReasonDto { Id = 2, RejectionReasonName = "Submitted incorrect information" }      
        };

        [Required(ErrorMessage = "Provide information about why another check is needed")]
        public string? SecondaryCheckComment { get; set; }

        [Required(ErrorMessage = "Select a reason for rejection")]
        public List<int>? SelectedReasonIds { get; set; }
        public List<RejectionReasonDto>? SelectedReasons { get; set; }
        public PublicInterestCheckDto? PublicInterestCheck { get; set; }
    }
}
